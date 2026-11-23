
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class EfOrderStore(AppDbContext db) : IAsyncOrderStore
{
  private readonly AppDbContext _db = db;

  private static Order ToDomain(OrderRow row)
  {
    var dto = new Core.Dto.OrderDto(
        row.Id,
        row.CustomerId,
        row.Status,
        row.Lines.Select(line => new OrderLineDto(
            line.ProductId,
            line.Name,
            line.Price,
            line.Quantity
        )).ToList(),
        row.CreatedAt
    );

    return Order.FromDto(dto);
  }

  private static OrderRow ToRow(Order order)
  {
    return new OrderRow
    {
      Id = order.Id,
      CustomerId = order.CustomerId,
      CreatedAt = order.CreatedAt,
      Status = order.Status,

      Lines = order.Lines.Select(line => new OrderLineRow
      {
        OrderId = order.Id,
        ProductId = line.ProductId,
        Name = line.Name,
        Price = line.Price,
        Quantity = line.Quantity
      }).ToList()
    };
  }

  public async Task<IReadOnlyList<Order>> ListAsync(
      CancellationToken ct = default)
  {
    var rows = await _db.Orders
        .AsNoTracking()
        .Include(o => o.Lines)
        .ToListAsync(ct);

    return rows.Select(ToDomain).ToList();
  }

  public async Task<Order?> GetByIdAsync(
      string id,
      CancellationToken ct = default)
  {
    var row = await _db.Orders
        .AsNoTracking()
        .Include(o => o.Lines)
        .SingleOrDefaultAsync(o => o.Id == id, ct);

    return row is null ? null : ToDomain(row);
  }

  public async Task AddAsync(
      Order order,
      CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(order);

    _db.Orders.Add(ToRow(order));

    try
    {
      await _db.SaveChangesAsync(ct);
    }
    catch (DbUpdateException ex)
        when (IsUniqueConstraintViolation(ex))
    {
      throw new DomainException($"Замовлення з ID={order.Id} вже існує.", ex);
    }
  }

  public async Task UpdateAsync(
      Order order,
      CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(order);

    var row = await _db.Orders
        .Include(o => o.Lines)
        .SingleOrDefaultAsync(o => o.Id == order.Id, ct);

    if (row is null)
      throw new DomainException($"Замовлення з ID={order.Id} не знайдено.");

    row.CustomerId = order.CustomerId;
    row.CreatedAt = order.CreatedAt;
    row.Status = order.Status;

    _db.OrderLines.RemoveRange(row.Lines);
    row.Lines.Clear();

    foreach (var line in order.Lines)
    {
      row.Lines.Add(new OrderLineRow
      {
        OrderId = order.Id,
        ProductId = line.ProductId,
        Name = line.Name,
        Price = line.Price,
        Quantity = line.Quantity
      });
    }

    await _db.SaveChangesAsync(ct);
  }

  public async Task<bool> RemoveAsync(
      string id,
      CancellationToken ct = default)
  {
    var row = await _db.Orders
        .SingleOrDefaultAsync(o => o.Id == id, ct);

    if (row is null)
      return false;

    _db.Orders.Remove(row);

    await _db.SaveChangesAsync(ct);

    return true;
  }

  private static bool IsUniqueConstraintViolation(
      DbUpdateException exception)
  {
    return exception.InnerException is SqliteException sqlite &&
        sqlite.SqliteExtendedErrorCode is 1555 or 2067;
  }
}
