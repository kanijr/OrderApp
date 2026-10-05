namespace Core.Dto;

public abstract record MixedItemDto;

public sealed record ProductItemDto(ProductDto Product)
    : MixedItemDto;

public sealed record CustomerItemDto(CustomerDto Customer)
    : MixedItemDto;