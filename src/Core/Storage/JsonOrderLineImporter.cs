using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonOrderLineImporter : IOrderLineImporter
{
  public async IAsyncEnumerable<OrderLine> LoadAsync(
      string path,
      [EnumeratorCancellation] CancellationToken ct = default)
  {
    ct.ThrowIfCancellationRequested();

    await using var stream = new FileStream(
        path,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        4096,
        useAsync: true);

    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    int index = 0;

    await foreach (var dto in
        JsonSerializer.DeserializeAsyncEnumerable<OrderLineDto>(
            stream,
            options,
            cancellationToken: ct))
    {
      ct.ThrowIfCancellationRequested();
      index++;

      if (dto is null)
        throw new DomainException(
            $"Запис {index}: значення null неприпустиме.");

      OrderLine line;

      try
      {
        line = OrderLine.FromDto(dto);
      }
      catch (ArgumentException ex)
      {
        throw new DomainException(
            $"Запис {index}: {ex.Message}", ex);
      }

      yield return line;
    }
  }
}