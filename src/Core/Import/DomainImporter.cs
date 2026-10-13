
using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImporter
{
  public static ImportResult<OrderLine> ToDomain(IEnumerable<OrderLineDto> source)
  {
    ArgumentNullException.ThrowIfNull(source);

    List<OrderLine> items = [];
    List<string> errors = [];

    int index = 0;

    foreach (OrderLineDto dto in source)
    {
      index++;

      try
      {
        items.Add(OrderLine.FromDto(dto));
      }
      catch (ArgumentException ex)
      {
        errors.Add($"Запис {index}: {ex.Message}");
      }
    }

    return new ImportResult<OrderLine>(items, errors);
  }
}
