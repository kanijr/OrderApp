using Core.Domain;

namespace Infrastructure.Persistence;

public sealed class OrderRow
{
  public string Id { get; set; } = "";

  public string CustomerId { get; set; } = "";

  public DateTimeOffset CreatedAt { get; set; }

  public OrderStatus Status { get; set; }

  public List<OrderLineRow> Lines { get; set; } = [];
}