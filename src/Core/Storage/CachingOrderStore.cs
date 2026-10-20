
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingOrderStore(IOrderStore inner) : IOrderStore
{
  private readonly IOrderStore _inner =
      inner ?? throw new ArgumentNullException(nameof(inner));

  private IReadOnlyList<Order>? _cachedList;
  private readonly Dictionary<string, Order?> _cachedOrders =
      new(StringComparer.OrdinalIgnoreCase);

  public IReadOnlyList<Order> List()
  {
    if (_cachedList is not null)
      return _cachedList;

    _cachedList = _inner.List();
    return _cachedList;
  }

  public Order? GetById(string id)
  {
    if (_cachedOrders.TryGetValue(id, out var cached))
      return cached;

    var order = _inner.GetById(id);
    _cachedOrders[id] = order;

    return order;
  }

  public void Add(Order order)
  {
    _inner.Add(order);
    ClearCache();
  }

  public void Update(Order order)
  {
    _inner.Update(order);
    ClearCache();
  }

  public bool Remove(string id)
  {
    bool removed = _inner.Remove(id);

    if (removed)
      ClearCache();

    return removed;
  }

  private void ClearCache()
  {
    _cachedList = null;
    _cachedOrders.Clear();
  }
}
