
using Core.Domain;

namespace Core.Dto;

public sealed record OrderDto(
    string Id,
    string CustomerId,
    OrderStatus Status,
    List<OrderLineDto> Lines
);
