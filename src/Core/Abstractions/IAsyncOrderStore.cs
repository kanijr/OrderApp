
using Core.Domain;

namespace Core.Abstractions;

public interface IAsyncOrderStore
{
  ValueTask<IReadOnlyList<Order>> ListAsync(
      CancellationToken ct = default);

  ValueTask<Order?> GetByIdAsync(
      string id,
      CancellationToken ct = default);

  ValueTask AddAsync(
      Order order,
      CancellationToken ct = default);

  ValueTask UpdateAsync(
      Order order,
      CancellationToken ct = default);

  ValueTask<bool> RemoveAsync(
      string id,
      CancellationToken ct = default);
}
