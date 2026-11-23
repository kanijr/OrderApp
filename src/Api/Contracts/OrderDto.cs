
using Core.Domain;

namespace Api.Contracts;

public sealed record OrderDto(
    string Id,
    string CustomerId,
    DateTimeOffset CreatedAt,
    string Status,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Total)
{
  public static OrderDto FromDomain(Order order) => new(
      order.Id,
      order.CustomerId,
      order.CreatedAt,
      order.Status.ToString(),
      order.Lines.Select(line => new OrderLineDto(
          line.ProductId,
          line.Name,
          line.Price,
          line.Quantity,
          line.Total
      )).ToList(),
      order.Total
  );
}

public sealed record OrderLineDto(
    string ProductId,
    string Name,
    decimal Price,
    int Quantity,
    decimal Total
);

public sealed record OrderTotalDto(
    string Id,
    decimal Total
);
