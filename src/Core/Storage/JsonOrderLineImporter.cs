using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonOrderLineImporter : IOrderLineImporter
{
  public async Task<IReadOnlyList<OrderLine>> LoadAsync(
      string path,
      CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    await using var stream = new FileStream(
        path,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        4096,
        useAsync: true);

    List<OrderLineDto> dtos;

    try
    {
      dtos = await JsonSerializer.DeserializeAsync<
          List<OrderLineDto>>(
              stream,
              new JsonSerializerOptions
              {
                PropertyNameCaseInsensitive = true
              },
              ct)
          ?? throw new DomainException(
              "Файл не містить списку рядків замовлення.");
    }
    catch (JsonException ex)
    {
      throw new DomainException(
          $"Некоректний формат JSON: {ex.Message}", ex);
    }

    var result = new List<OrderLine>(dtos.Count);

    for (int i = 0; i < dtos.Count; i++)
    {
      ct.ThrowIfCancellationRequested();

      try
      {
        result.Add(OrderLine.FromDto(dtos[i]));
      }
      catch (ArgumentException ex)
      {
        throw new DomainException(
            $"Запис {i + 1}: {ex.Message}", ex);
      }
    }

    return result;
  }
}