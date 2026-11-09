using Core.Abstractions;
using Core.Domain;
using Core.Events;


namespace Core.Services;

public sealed class OrderService(
    IOrderStore store,
    IEnumerable<INotificationSink>? sinks = null)
{
  private readonly IOrderStore _store =
      store ?? throw new ArgumentNullException(nameof(store));

  private readonly IReadOnlyList<INotificationSink> _sinks = (sinks ?? []).ToList();

  public event EventHandler<OrderChangedEventArgs>? Changed;

  public async Task<int> ImportAsync(
    IOrderLineImporter importer,
    string path,
    string orderId,
    CancellationToken ct = default)
  {
    ArgumentNullException.ThrowIfNull(importer);

    ct.ThrowIfCancellationRequested();

    var order = _store.GetById(orderId)
        ?? throw new ArgumentException(
            $"Замовлення з ID={orderId} не знайдено.",
            nameof(orderId));

    if (order.Status != OrderStatus.Draft)
      throw new DomainException(
          $"Замовлення {orderId} вже не є чернеткою.");

    var lines = await importer.LoadAsync(path, ct);

    int added = 0;

    foreach (var line in lines)
    {
      ct.ThrowIfCancellationRequested();

      AddLine(
          orderId,
          line.ProductId,
          line.Name,
          line.Price,
          line.Quantity);

      added++;
    }

    return added;
  }

  private void OnChanged(Order order, OrderChangeKind kind)
  {
    var e = new OrderChangedEventArgs(
        order.Id, kind, order.Lines.Count, order.Total);

    foreach (var sink in _sinks)
      sink.Notify(e);

    Changed?.Invoke(this, e);
  }
  public Order CreateOrder(string customerId)
  {
    int draftCount = _store.List().Count(o =>
        o.CustomerId == customerId &&
        o.Status == OrderStatus.Draft);

    if (draftCount >= 5)
      throw new DomainException(
          "Покупець не може мати більше 5 чернеток замовлень.");

    var order = Order.Create(customerId);

    _store.Add(order);

    OnChanged(order, OrderChangeKind.OrderCreated);
    return order;
  }

  public void AddLine(
      string orderId,
      string productId,
      string name,
      decimal price,
      int quantity)
  {
    var order = _store.GetById(orderId)
        ?? throw new InvalidOperationException(
            $"Замовлення з ID={orderId} не знайдено.");

    order.AddLine(productId, name, price, quantity);

    _store.Update(order);
    OnChanged(order, OrderChangeKind.LineAdded);
  }

  public void ConfirmOrder(string orderId)
  {
    var order = _store.GetById(orderId)
        ?? throw new InvalidOperationException(
            $"Замовлення з ID={orderId} не знайдено.");

    order.Confirm();

    _store.Update(order);
    OnChanged(order, OrderChangeKind.OrderConfirmed);
  }

  public void CancelOrder(string orderId)
  {
    var order = _store.GetById(orderId)
        ?? throw new InvalidOperationException(
            $"Замовлення з ID={orderId} не знайдено.");

    order.Cancel();

    _store.Update(order);
  }

  public IReadOnlyList<Order> FindAll(Func<Order, bool> predicate)
  {
    ArgumentNullException.ThrowIfNull(predicate);
    return _store.List().Where(predicate).ToList();
  }

  public IReadOnlyList<Order> All() =>
      _store.List();

  public Order? Find(string id) =>
      _store.GetById(id);
}
