using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
  public static ImportResult<ProductDto> Load(string path)
  {
    var items = new List<ProductDto>();
    var errors = new List<string>();

    string json = File.ReadAllText(path);

    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    JsonDocument document;

    try
    {
      document = JsonDocument.Parse(json);
    }
    catch (JsonException ex)
    {
      errors.Add($"Некоректний JSON: {ex.Message}");
      return new ImportResult<ProductDto>(items, errors);
    }

    using (document)
    {
      if (document.RootElement.ValueKind != JsonValueKind.Array)
      {
        errors.Add("Кореневий елемент JSON повинен бути масивом");
        return new ImportResult<ProductDto>(items, errors);
      }

      int number = 0;

      foreach (JsonElement element in document.RootElement.EnumerateArray())
      {
        number++;

        try
        {
          ProductDto? product =
              element.Deserialize<ProductDto>(options);

          if (product is null)
          {
            errors.Add(
                $"запис {number}: не вдалося десеріалізувати");
            continue;
          }

          if (string.IsNullOrWhiteSpace(product.Id))
          {
            errors.Add(
                $"запис {number}: ID товару порожній");
            continue;
          }

          if (string.IsNullOrWhiteSpace(product.Name))
          {
            errors.Add(
                $"запис {number}: назва товару порожня");
            continue;
          }

          if (product.Price < 0)
          {
            errors.Add(
                $"запис {number}: ціна не може бути від'ємною");
            continue;
          }

          items.Add(product);
        }
        catch (JsonException ex)
        {
          errors.Add(
              $"запис {number}: {ex.Message}");
        }
      }
    }

    return new ImportResult<ProductDto>(items, errors);
  }
}