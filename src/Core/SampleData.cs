using Core.Domain;

namespace Core;

public static class SampleData
{
  public static IEnumerable<Order> Orders()
  {
    return
    [
        Order.FromDto(new ("O-001", "C-001", OrderStatus.Confirmed,
            [
                new("P-001", "Ноутбук Lenovo", 29999.99m, 1),
                new("P-002", "Миша Logitech", 799.50m, 1)
            ])),

        Order.FromDto(new ("O-002", "C-002", OrderStatus.Draft,
            [new("P-002", "Миша Logitech", 799.50m, 2)])),

        Order.FromDto(new ("O-003", "C-003", OrderStatus.Confirmed,
            [new("P-003", "Клавіатура", 1499.00m, 1)])),

        Order.FromDto(new ("O-004", "C-001", OrderStatus.Draft,
            [new("P-004", "Монітор Samsung", 6499.00m, 1)])),

        Order.FromDto(new ("O-005", "C-004", OrderStatus.Confirmed,
            [new("P-005", "Навушники Sony", 2499.00m, 1)])),

        Order.FromDto(new ("O-006", "C-002", OrderStatus.Cancelled,
            [new("P-006", "Вебкамера Logitech", 1999.00m, 1)])),

        Order.FromDto(new ("O-007", "C-005", OrderStatus.Draft,
            [new("P-007", "USB-накопичувач", 399.00m, 3)])),

        Order.FromDto(new ("O-008", "C-003", OrderStatus.Confirmed,
            [new("P-008", "SSD Kingston", 2899.00m, 1)])),

        Order.FromDto(new ("O-009", "C-001", OrderStatus.Confirmed,
            [new("P-009", "Оперативна пам'ять", 1799.00m, 2)])),

        Order.FromDto(new ("O-010", "C-006", OrderStatus.Draft,
            [new("P-010", "Роутер TP-Link", 1299.00m, 1)])),

        Order.FromDto(new ("O-011", "C-004", OrderStatus.Cancelled,
            [new("P-011", "Кабель HDMI", 249.00m, 2)])),

        Order.FromDto(new ("O-012", "C-002", OrderStatus.Confirmed,
            [
                new("P-012", "Зарядний пристрій", 899.00m, 1),
                new("P-013", "Павербанк", 1599.00m, 1)
            ])),

        Order.FromDto(new ("O-013", "C-005", OrderStatus.Draft,
            [new("P-013", "Павербанк", 1599.00m, 1)])),

        Order.FromDto(new ("O-014", "C-003", OrderStatus.Confirmed,
            [new("P-014", "Мікрофон", 2199.00m, 1)])),

        Order.FromDto(new ("O-015", "C-007", OrderStatus.Cancelled,
            [new("P-015", "Килимок для миші", 299.00m, 2)])),

        Order.FromDto(new ("O-016", "C-001", OrderStatus.Confirmed,
            [new("P-016", "Ноутбук ASUS", 25999.00m, 1)])),

        Order.FromDto(new ("O-017", "C-006", OrderStatus.Draft,
            [new("P-017", "Монітор LG", 6499.00m, 1)])),

        Order.FromDto(new ("O-018", "C-007", OrderStatus.Cancelled,
            [new("P-018", "Навушники JBL", 2499.00m, 1)]))
    ];
  }
}