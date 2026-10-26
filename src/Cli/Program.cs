using Core.Services;
using Core.Abstractions;
using Core.Storage;
using Core.Events;

IOrderStore store = StoreFactory.Create(args);
var service = new OrderService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");

if (args.Contains("--file"))
{
    string dataPath = Path.Combine(
        AppContext.BaseDirectory, "data", "orders.json");

    Console.WriteLine($"Файл з даними: {dataPath}");
}

string logPath = Path.Combine(
    AppContext.BaseDirectory, "logs", "app.log");

Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    string line =
        $"{e.At:HH:mm:ss} {e.Kind,-15} " +
        $"{e.OrderId} Рядків: {e.LinesCount} " +
        $"Сума: {e.Total:F2} грн";

    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

service.Changed += OnOrderChanged;

int changes = 0;
service.Changed += (_, _) => changes++;


Console.WriteLine("\nОперації із замовленням");

var created = service.CreateOrder("C-016");

service.AddLine(
    created.Id, "P-001", "Ноутбук Lenovo", 29999.99m, 1);

service.AddLine(
    created.Id, "P-002", "Миша Logitech", 799.50m, 2);


service.ConfirmOrder(created.Id);
Console.WriteLine($"Подій отримано: {changes}");


Console.WriteLine("\nПеревірка інваріанта");

try
{
    service.AddLine(
        created.Id, "P-003", "Mouse", 500m, 1);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

Console.WriteLine($"Подій отримано: {changes}");


Console.WriteLine("\nВідписка від події");

service.Changed -= OnOrderChanged;


var secondOrder = service.CreateOrder("C-017");

Console.WriteLine($"Створено замовлення: {secondOrder.Id}");
Console.WriteLine($"Подій отримано: {changes}");


Console.WriteLine("\nПошук замовлень:");

var expensive = service.FindAll(o => o.Total > 5000m);

Console.WriteLine($"Замовлень понад 5000 грн: {expensive.Count}");

foreach (var order in expensive)
{
    Console.WriteLine(
        $"ID: {order.Id}, " +
        $"Статус: {order.Status}, " +
        $"Сума: {order.Total}");
}


return 0;
