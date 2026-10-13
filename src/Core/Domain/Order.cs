
using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
  private readonly List<OrderLine> _lines = [];

  public string Id { get; }
  public string CustomerId { get; }
  public bool IsConfirmed { get; private set; }

  public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
  public decimal Total => _lines.Sum(line => line.Total);

  private Order(string id, string customerId)
  {
    Id = id;
    CustomerId = customerId;
  }

  public static Order Create(string customerId)
  {
    return CreateWithId(Guid.NewGuid().ToString("N"), customerId);
  }

  private static Order CreateWithId(string id, string customerId)
  {
    if (string.IsNullOrWhiteSpace(id))
      throw new ArgumentException("Номер замовлення обов'язковий", nameof(id));

    if (string.IsNullOrWhiteSpace(customerId))
      throw new ArgumentException("Ідентифікатор покупця обов'язковий", nameof(customerId));

    return new Order(id.Trim(), customerId.Trim());
  }

  public void AddLine(string productId, string name, decimal price, int quantity)
  {
    if (IsConfirmed)
      throw new InvalidOperationException(
          $"Замовлення {Id} вже підтверджене, додавати товари не можна");

    OrderLine line = OrderLine.Create(productId, name, price, quantity);
    _lines.Add(line);
  }

  public void Confirm()
  {
    if (IsConfirmed)
      throw new InvalidOperationException(
          $"Замовлення {Id} вже підтверджене");

    if (_lines.Count == 0)
      throw new InvalidOperationException(
          $"Не можна підтвердити порожнє замовлення {Id}");

    IsConfirmed = true;
  }

  public OrderDto ToDto() => new(
      Id,
      CustomerId,
      IsConfirmed,
      _lines.Select(line => line.ToDto()).ToList()
  );

  public static Order FromDto(OrderDto dto)
  {
    ArgumentNullException.ThrowIfNull(dto);

    Order order = CreateWithId(dto.Id, dto.CustomerId);

    if (dto.Lines is null)
      throw new ArgumentException("Список товарів відсутній", nameof(dto));

    foreach (OrderLineDto line in dto.Lines)
    {
      OrderLine validLine = OrderLine.FromDto(line);

      order.AddLine(
          validLine.ProductId,
          validLine.Name,
          validLine.Price,
          validLine.Quantity
      );
    }

    if (dto.IsConfirmed)
      order.Confirm();

    return order;
  }
}
