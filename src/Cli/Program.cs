using Core.Services;
using Core.Abstractions;
using Core.Storage;
using Core;

bool useFile = args.Contains("--file");

string dataPath = Path.Combine(
    AppContext.BaseDirectory, "data", "orders.json");

IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());

var service = new OrderService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile) Console.WriteLine($"Файл з даними: {dataPath}");
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
    Console.WriteLine($"Очікувана помилка: {ex.Message}");
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

return 0;
