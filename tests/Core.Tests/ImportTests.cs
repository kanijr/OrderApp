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
    var service = new OrderService(new InMemoryOrderStore());
    var order = service.CreateOrder("C-001");

    using var cts = new CancellationTokenSource();
    cts.Cancel();

    await Assert.ThrowsAnyAsync<OperationCanceledException>(
        () => service.ImportAsync(
            new JsonOrderLineImporter(),
            "any.json",
            order.Id,
            cts.Token));

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
          () => importer.LoadAsync(path));

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
    var service = new OrderService(new InMemoryOrderStore());
    var order = service.CreateOrder("C-001");

    var importer = new FakeOrderLineImporter(TestData.Lines());

    int added = await service.ImportAsync(
        importer, "unused.json", order.Id);

    Assert.Equal(3, added);
    Assert.Equal(3, order.Lines.Count);
    Assert.Equal(2900m, order.Total);
  }

  private sealed class FakeOrderLineImporter(
      IReadOnlyList<OrderLine> lines) : IOrderLineImporter
  {
    public Task<IReadOnlyList<OrderLine>> LoadAsync(
        string path,
        CancellationToken ct = default)
    {
      ct.ThrowIfCancellationRequested();
      return Task.FromResult(lines);
    }
  }
}