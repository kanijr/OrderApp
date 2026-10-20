using Core.Services;
using Core.Abstractions;
using Core.Storage;
using Core.Domain;
using System.Buffers;

IOrderStore store = StoreFactory.Create(args);
var service = new OrderService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

if (args.Contains("--file"))
{
    string dataPath = Path.Combine(
        AppContext.BaseDirectory, "data", "orders.json");

    Console.WriteLine($"Файл з даними: {dataPath}");
}

Console.WriteLine($"Початкових замовлень: {service.All().Count}");

var created = service.CreateOrder("C-016");
Console.WriteLine($"\nСтворено замовлення: {created.Id}");

service.AddLine(
    created.Id, "P-001", "Ноутбук Lenovo", 29999.99m, 1);

service.AddLine(
    created.Id, "P-002", "Миша Logitech", 799.50m, 2);

var found = service.Find(created.Id);

if (found is not null)
{
    Console.WriteLine(
        $"Знайдено: {found.Id}, " +
        $"Рядків: {found.Lines.Count}, Сума: {found.Total}");
}

service.ConfirmOrder(created.Id);
Console.WriteLine("Замовлення підтверджено.");

try
{
    service.ConfirmOrder("non-existing-id");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Не існуюче id: {ex.Message}");
}

try
{
    service.CancelOrder(created.Id);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"\nСпроба скасувати після підтвердження: {ex.Message}");
}


Console.WriteLine("\nСписок замовлень:");

foreach (var order in service.All())
{
    Console.WriteLine(
        $"ID: {order.Id}, " +
        $"Покупець: {order.CustomerId}, " +
        $"Статус: {order.Status}, " +
        $"Сума: {order.Total}");
}

if (args.Contains("--cache"))
{
    Console.WriteLine("\nПеревірка кешування:");

    var first = store.List();
    var second = store.List();

    Console.WriteLine(
        $"Повторно використано кешований список: " +
        $"{ReferenceEquals(first, second)}");
}

Console.WriteLine("\nПошук замовлень:");

var confirmed = service.Search(o =>
    o.Status == OrderStatus.Confirmed);

Console.WriteLine($"Підтверджених: {confirmed.Count}");

var expensive = service.Search(o => o.Total > 5000m);

Console.WriteLine($"Замовлень понад 5000 грн: {expensive.Count}");

var customerOrders = service.Search(o =>
    o.CustomerId == "C-016");

Console.WriteLine("\nЗамовлення покупця C-016:");

foreach (var order in customerOrders)
{
    Console.WriteLine(
        $"ID: {order.Id}, " +
        $"Статус: {order.Status}, " +
        $"Сума: {order.Total}");
}


return 0;
