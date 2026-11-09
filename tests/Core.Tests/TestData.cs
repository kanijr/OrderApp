using Core.Domain;

namespace Core.Tests;

public static class TestData
{
  public static IReadOnlyList<Order> Orders()
  {
    var first = Order.Create("C-001");
    first.AddLine("P-001", "Ноутбук", 1000m, 2);

    var second = Order.Create("C-002");
    second.AddLine("P-002", "Миша", 200m, 3);

    var third = Order.Create("C-001");
    third.AddLine("P-003", "Клавіатура", 500m, 1);

    return [first, second, third];
  }

  public static IReadOnlyList<OrderLine> Lines() =>
  [
      OrderLine.Create("P-001", "Ноутбук", 1000m, 1),
        OrderLine.Create("P-002", "Миша", 200m, 2),
        OrderLine.Create("P-003", "Клавіатура", 500m, 3)
  ];
}