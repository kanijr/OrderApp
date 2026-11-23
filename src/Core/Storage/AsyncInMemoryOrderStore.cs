
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class AsyncInMemoryOrderStore(
    IEnumerable<Order>? seed = null) : IAsyncOrderStore
{
  private readonly Dictionary<string, Order> _items =
      (seed ?? []).ToDictionary(
          o => o.Id,
          StringComparer.OrdinalIgnoreCase);

  private readonly object _sync = new();

  public Task<IReadOnlyList<Order>> ListAsync(
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    lock (_sync)
    {
      IReadOnlyList<Order> result =
          _items.Values.ToList();

      return Task.FromResult(result);
    }
  }

  public Task<Order?> GetByIdAsync(
      string id,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    lock (_sync)
    {
      Order? result = _items.GetValueOrDefault(id);

      return Task.FromResult(result);
    }
  }

  public Task AddAsync(
      Order order,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    ArgumentNullException.ThrowIfNull(order);

    lock (_sync)
    {
      if (_items.ContainsKey(order.Id))
        throw new InvalidOperationException(
            $"Замовлення з ID={order.Id} вже існує.");

      _items.Add(order.Id, order);
    }

    return Task.CompletedTask;
  }

  public Task UpdateAsync(
      Order order,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();
    ArgumentNullException.ThrowIfNull(order);

    lock (_sync)
    {
      _items[order.Id] = order;
    }

    return Task.CompletedTask;
  }

  public Task<bool> RemoveAsync(
      string id,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    lock (_sync)
    {
      bool removed = _items.Remove(id);

      return Task.FromResult(removed);
    }
  }
}
