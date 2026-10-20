using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService(IOrderStore store)
{
  private readonly IOrderStore _store =
      store ?? throw new ArgumentNullException(nameof(store));

  public Order CreateOrder(string customerId)
  {
    int draftCount = _store.List().Count(o =>
        o.CustomerId == customerId &&
        o.Status == OrderStatus.Draft);

    if (draftCount >= 5)
      throw new InvalidOperationException(
          "Покупець не може мати більше 5 чернеток замовлень.");

    var order = Order.Create(customerId);

    _store.Add(order);

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
  }

  public void ConfirmOrder(string orderId)
  {
    var order = _store.GetById(orderId)
        ?? throw new InvalidOperationException(
            $"Замовлення з ID={orderId} не знайдено.");

    order.Confirm();

    _store.Update(order);
  }

  public IReadOnlyList<Order> All() =>
      _store.List();

  public Order? Find(string id) =>
      _store.GetById(id);
}
