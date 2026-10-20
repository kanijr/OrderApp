
using Core.Domain;

namespace Core;

public static class SampleData
{
  public static IEnumerable<Order> Orders()
  {
    var o1 = Order.Create("C-001");
    o1.AddLine("P-001", "Ноутбук Lenovo", 29999.99m, 1);

    var o2 = Order.Create("C-002");
    o2.AddLine("P-002", "Миша Logitech", 799.50m, 2);

    var o3 = Order.Create("C-003");
    o3.AddLine("P-003", "Клавіатура", 1499.00m, 1);
    o3.Confirm();

    var o4 = Order.Create("C-004");
    o4.AddLine("P-004", "Монітор Samsung", 6499.00m, 1);

    var o5 = Order.Create("C-005");
    o5.AddLine("P-005", "Навушники Sony", 2499.00m, 1);
    o5.Confirm();

    var o6 = Order.Create("C-006");
    o6.AddLine("P-006", "Вебкамера Logitech", 1999.00m, 1);

    var o7 = Order.Create("C-007");
    o7.AddLine("P-007", "USB-накопичувач", 399.00m, 3);

    var o8 = Order.Create("C-008");
    o8.AddLine("P-008", "SSD Kingston", 2899.00m, 1);
    o8.Confirm();

    var o9 = Order.Create("C-009");
    o9.AddLine("P-009", "Оперативна пам'ять", 1799.00m, 2);

    var o10 = Order.Create("C-010");
    o10.AddLine("P-010", "Роутер TP-Link", 1299.00m, 1);

    var o11 = Order.Create("C-011");
    o11.AddLine("P-011", "Кабель HDMI", 249.00m, 2);
    o11.Confirm();

    var o12 = Order.Create("C-012");
    o12.AddLine("P-012", "Зарядний пристрій", 899.00m, 1);

    var o13 = Order.Create("C-013");
    o13.AddLine("P-013", "Павербанк", 1599.00m, 1);

    var o14 = Order.Create("C-014");
    o14.AddLine("P-014", "Мікрофон", 2199.00m, 1);
    o14.Confirm();

    var o15 = Order.Create("C-015");
    o15.AddLine("P-015", "Килимок для миші", 299.00m, 2);

    return
    [
        o1, o2, o3, o4, o5,
            o6, o7, o8, o9, o10,
            o11, o12, o13, o14, o15
    ];
  }
}
