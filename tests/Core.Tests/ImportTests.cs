using System.Runtime.CompilerServices;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class ImportTests
{
  [Fact]
  public async Task ImportAsync_WhenAlreadyCancelled_Throws()
  {
    var service = new OrderService(
        new InMemoryOrderStore());

    var order = service.CreateOrder("C-001");

    using var cts = new CancellationTokenSource();
    cts.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(
        () => service.ImportAsync(
            new FakeImporter(),
            "unused.json",
            order.Id,
            ct: cts.Token));

    Assert.Empty(order.Lines);
  }

  [Fact]
  public async Task LoadAsync_WithInvalidQuantity_ThrowsDomainException()
  {
    string path = Path.GetTempFileName();

    try
    {
      await File.WriteAllTextAsync(path,
          """
                [
                  {
                    "productId": "P-001",
                    "name": "Товар",
                    "price": 100,
                    "quantity": -5
                  }
                ]
                """);

      var importer = new JsonOrderLineImporter();

      var ex = await Assert.ThrowsAsync<DomainException>(
          async () =>
          {
            await foreach (var line in importer.LoadAsync(path)) { }
          });

      Assert.Contains("Запис 1", ex.Message);
    }
    finally
    {
      File.Delete(path);
    }
  }

  [Fact]
  public async Task ImportAsync_AddsLinesAndCalculatesTotal()
  {
    var service = new OrderService(
        new InMemoryOrderStore());

    var order = service.CreateOrder("C-001");

    var importer = new FakeImporter();

    int added = await service.ImportAsync(
        importer,
        "unused.json",
        order.Id);

    Assert.Equal(3, added);
    Assert.Equal(3, order.Lines.Count);
    Assert.Equal(2900m, order.Total);
  }

  [Fact]
  public async Task ImportAsync_WhenTimeout_Throws()
  {
    var service = new OrderService(
        new InMemoryOrderStore());

    var order = service.CreateOrder("C-001");

    using var userCts = new CancellationTokenSource();
    using var timeoutCts = new CancellationTokenSource();

    using var linkedCts =
        CancellationTokenSource.CreateLinkedTokenSource(
            userCts.Token,
            timeoutCts.Token);

    timeoutCts.CancelAfter(
        TimeSpan.FromMilliseconds(100));

    await Assert.ThrowsAnyAsync<OperationCanceledException>(
        () => service.ImportAsync(
            new SlowImporter(),
            "unused.json",
            order.Id,
            ct: linkedCts.Token));

    Assert.True(timeoutCts.IsCancellationRequested);
    Assert.False(userCts.IsCancellationRequested);
    Assert.Empty(order.Lines);
  }

  private sealed class FakeImporter : IOrderLineImporter
  {
    public async IAsyncEnumerable<OrderLine> LoadAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
      foreach (var line in TestData.Lines())
      {
        ct.ThrowIfCancellationRequested();
        await Task.Yield();
        yield return line;
      }
    }
  }

  private sealed class SlowImporter : IOrderLineImporter
  {
    public async IAsyncEnumerable<OrderLine> LoadAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
      await Task.Delay(Timeout.InfiniteTimeSpan, ct);
      yield break;
    }
  }
}