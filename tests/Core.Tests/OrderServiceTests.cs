using Core.Domain;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class OrderServiceTests
{
  [Fact]
  public void AddLine_ToConfirmedOrder_Throws_AndKeepsState()
  {
    var service = new OrderService(new InMemoryOrderStore());

    var order = service.CreateOrder("C-001");
    service.AddLine(order.Id, "P-001", "Ноутбук", 1000m, 2);
    service.ConfirmOrder(order.Id);

    decimal totalBefore = order.Total;
    int countBefore = order.Lines.Count;

    Assert.Throws<DomainException>(() =>
        service.AddLine(order.Id, "P-002", "Миша", 100m, 1));

    Assert.Equal(totalBefore, order.Total);
    Assert.Equal(countBefore, order.Lines.Count);
  }

  [Fact]
  public void AddLine_RaisesChangedExactlyOnce()
  {
    var service = new OrderService(new InMemoryOrderStore());
    var order = service.CreateOrder("C-002");

    var events = new List<OrderChangedEventArgs>();

    service.Changed += (_, e) => events.Add(e);

    service.AddLine(order.Id, "P-001", "Ноутбук", 1000m, 1);

    Assert.Single(events);
    Assert.Equal(OrderChangeKind.LineAdded, events[0].Kind);
  }

  [Theory]
  [InlineData(100, 2, 200)]
  [InlineData(250, 3, 750)]
  [InlineData(0, 5, 0)]
  public void AddLine_CalculatesCorrectTotal(
      int price, int quantity, int expected)
  {
    var service = new OrderService(new InMemoryOrderStore());
    var order = service.CreateOrder("C-003");

    service.AddLine(
        order.Id, "P-001", "Товар", price, quantity);

    Assert.Equal((decimal)expected, order.Total);
  }

  [Fact]
  public void TopOrders_IsDeterministic()
  {
    var first = Order.Create("C-001");
    first.AddLine("P-001", "Товар A", 100m, 2);

    var second = Order.Create("C-002");
    second.AddLine("P-002", "Товар B", 300m, 1);

    var orders = new[] { first, second };

    var result1 = ReportService.TopOrders(orders, 2);
    var result2 = ReportService.TopOrders(orders, 2);

    Assert.Equal(
        result1.Select(o => o.OrderId),
        result2.Select(o => o.OrderId));

    Assert.Equal(second.Id, result1[0].OrderId);
  }
}