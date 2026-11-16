
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public static class OrderJsonSerializer
{
  private static readonly JsonSerializerOptions Options = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
  };

  public static List<Order> Load(string path)
  {
    if (!File.Exists(path))
      throw new FileNotFoundException("Файл замовлень не знайдено.", path);

    try
    {
      var json = File.ReadAllText(path);

      var dtos = JsonSerializer.Deserialize<List<OrderDto>>(json, Options)
          ?? throw new JsonException("Коренем JSON повинен бути масив замовлень.");

      if (dtos.Any(dto => dto is null))
        throw new InvalidDataException("JSON містить null замовлення.");

      var orders = dtos.Select(Order.FromDto).ToList();

      if (orders.Select(o => o.Id)
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .Count() != orders.Count)
      {
        throw new InvalidDataException("JSON містить повторювані ID замовлень.");
      }

      return orders;
    }
    catch (JsonException ex)
    {
      throw new InvalidDataException($"Некоректний JSON замовлень: {ex.Message}", ex);
    }
    catch (DomainException ex)
    {
      throw new InvalidDataException($"Некоректні дані замовлення: {ex.Message}", ex);
    }
    catch (ArgumentException ex)
    {
      throw new InvalidDataException($"Некоректні дані замовлення: {ex.Message}", ex);
    }
  }

  public static void Save(string path, IEnumerable<Order> orders)
  {
    var fullPath = Path.GetFullPath(path);

    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

    var tempPath = fullPath + ".tmp";

    var dtos = orders.Select(o => o.ToDto()).ToList();

    try
    {
      File.WriteAllText(tempPath, JsonSerializer.Serialize(dtos, Options));

      File.Move(tempPath, fullPath, overwrite: true);
    }
    finally
    {
      if (File.Exists(tempPath))
        File.Delete(tempPath);
    }
  }
}
