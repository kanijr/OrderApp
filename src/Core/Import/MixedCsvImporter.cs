using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
  private const char Separator = ';';

  public static ImportResult<MixedItemDto> Load(string path)
  {
    var items = new List<MixedItemDto>();
    var errors = new List<string>();

    string[] lines = File.ReadAllLines(path, Encoding.UTF8);

    for (int i = 0; i < lines.Length; i++)
    {
      int number = i + 1;
      string line = lines[i];

      if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
        continue;

      if (number == 1 &&
          line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
        continue;

      switch (ParseLine(line))
      {
        case ParseOk ok:
          items.Add(ok.Value);
          break;

        case ParseFailed failed:
          errors.Add($"рядок {number}: {failed.Reason}");
          break;
      }
    }

    return new ImportResult<MixedItemDto>(items, errors);
  }

  private static ParseOutcome ParseLine(string line)
  {
    string[] parts =
        line.Split(Separator, StringSplitOptions.TrimEntries);

    return parts switch
    {
      { Length: < 4 }
          => new ParseFailed(
              $"очікую 4 колонки, отримав {parts.Length}"),

      ["P", "", _, _]
          => new ParseFailed("ID товару порожній"),

      ["P", _, "", _]
          => new ParseFailed("назва товару порожня"),

      ["P", _, _, var price] when
          !decimal.TryParse(
              price,
              NumberStyles.Number,
              CultureInfo.InvariantCulture,
              out decimal p)
          || p < 0
          => new ParseFailed(
              $"ціна '{price}' не є коректним невід'ємним числом"),

      ["P", var id, var name, var price]
          => new ParseOk(
              new ProductItemDto(
                  new ProductDto(
                      id,
                      name,
                      decimal.Parse(
                          price,
                          NumberStyles.Number,
                          CultureInfo.InvariantCulture)))),

      ["C", "", _, _]
          => new ParseFailed("ID покупця порожній"),

      ["C", _, "", _]
          => new ParseFailed("ім'я покупця порожнє"),

      ["C", var id, var name, var email]
          => new ParseOk(
              new CustomerItemDto(
                  new CustomerDto(
                      id,
                      name,
                      string.IsNullOrWhiteSpace(email)
                          ? null
                          : email))),

      [var type, _, _, _]
          => new ParseFailed(
              $"невідомий тип запису '{type}'"),

      _ => new ParseFailed(
          $"занадто багато колонок: {parts.Length}")
    };
  }

  private abstract record ParseOutcome;

  private sealed record ParseOk(MixedItemDto Value)
      : ParseOutcome;

  private sealed record ParseFailed(string Reason)
      : ParseOutcome;
}