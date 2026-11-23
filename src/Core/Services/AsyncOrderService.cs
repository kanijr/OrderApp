
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Services;

public sealed class AsyncOrderService(IAsyncOrderStore store)
{
  private readonly IAsyncOrderStore _store = store;

  public async Task<Order> CreateOrderAsync(
      string customerId,
      IReadOnlyList<OrderLineDto> lines,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    if (string.IsNullOrWhiteSpace(customerId))
      throw new DomainException(
          "Ідентифікатор покупця обов'язковий.");

    ArgumentNullException.ThrowIfNull(lines);

    if (lines.Count == 0)
      throw new DomainException(
          "Замовлення повинно містити хоча б один товар.");

    customerId = customerId.Trim();

    var orders = await _store.ListAsync(ct);

    int draftCount = orders.Count(o =>
        o.CustomerId == customerId &&
        o.Status == OrderStatus.Draft);

    if (draftCount >= 5)
      throw new DomainException(
          "Покупець не може мати більше 5 чернеток замовлень.");

    var order = Order.Create(customerId);

    foreach (var line in lines)
    {
      ct.ThrowIfCancellationRequested();

      order.AddLine(
          line.ProductId,
          line.Name,
          line.Price,
          line.Quantity);
    }

    await _store.AddAsync(order, ct);

    return order;
  }

  public async Task<Order?> ConfirmOrderAsync(
      string id,
      CancellationToken ct = default)
  {
    var order = await _store.GetByIdAsync(id, ct);

    if (order is null)
      return null;

    ct.ThrowIfCancellationRequested();

    order.Confirm();

    await _store.UpdateAsync(order, ct);

    return order;
  }

  public async Task<Order?> CancelOrderAsync(
      string id,
      CancellationToken ct = default)
  {
    var order = await _store.GetByIdAsync(id, ct);

    if (order is null)
      return null;

    ct.ThrowIfCancellationRequested();

    order.Cancel();

    await _store.UpdateAsync(order, ct);

    return order;
  }
}
