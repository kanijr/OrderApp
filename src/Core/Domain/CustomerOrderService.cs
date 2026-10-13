
namespace Core.Domain;

public static class CustomerOrderService
{
  public static Order CreateOrder(Customer customer, IEnumerable<Order> orders)
  {
    ArgumentNullException.ThrowIfNull(customer);
    ArgumentNullException.ThrowIfNull(orders);

    int draftCount = orders.Count(o =>
      o.CustomerId == customer.Id &&
      o.Status == OrderStatus.Draft);

    if (draftCount >= 5)
      throw new InvalidOperationException(
        $"Покупець {customer.Id} вже має {draftCount} чернеток замовлень. Максимум — 5");

    return Order.Create(customer.Id);
  }
}
