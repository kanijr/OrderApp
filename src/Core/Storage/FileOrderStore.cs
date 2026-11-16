using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
  private readonly Dictionary<string, Order> _cache =
      new(StringComparer.OrdinalIgnoreCase);

  private readonly string _path = Path.GetFullPath(path);
  private bool _loaded;

  private void EnsureLoaded()
  {
    if (_loaded) return;

    if (File.Exists(_path))
    {
      foreach (var order in OrderJsonSerializer.Load(_path))
        _cache.Add(order.Id, order);
    }

    _loaded = true;
  }

  private void Flush() => OrderJsonSerializer.Save(_path, _cache.Values);

  public IReadOnlyList<Order> List()
  {
    EnsureLoaded();
    return _cache.Values.ToList();
  }

  public Order? GetById(string id)
  {
    EnsureLoaded();
    return _cache.GetValueOrDefault(id);
  }

  public void Add(Order order)
  {
    ArgumentNullException.ThrowIfNull(order);
    EnsureLoaded();

    if (_cache.ContainsKey(order.Id))
      throw new InvalidOperationException(
          $"Замовлення з ID={order.Id} вже існує.");

    _cache.Add(order.Id, order);
    Flush();
  }

  public void Update(Order order)
  {
    ArgumentNullException.ThrowIfNull(order);
    EnsureLoaded();
    _cache[order.Id] = order;
    Flush();
  }

  public bool Remove(string id)
  {
    EnsureLoaded();

    if (!_cache.Remove(id))
      return false;

    Flush();
    return true;
  }

  public void ReplaceAll(IEnumerable<Order> orders)
  {
    ArgumentNullException.ThrowIfNull(orders);

    var replacement = orders.ToDictionary(
        o => o.Id, StringComparer.OrdinalIgnoreCase);

    OrderJsonSerializer.Save(_path, replacement.Values);

    _cache.Clear();

    foreach (var pair in replacement)
      _cache.Add(pair.Key, pair.Value);

    _loaded = true;
  }
}
