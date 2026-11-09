using Core.Domain;

namespace Core.Abstractions;

public interface IOrderLineImporter
{
  IAsyncEnumerable<OrderLine> LoadAsync(
      string path,
      CancellationToken ct = default);
}