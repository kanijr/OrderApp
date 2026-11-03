using Core.Services;
using Core.Abstractions;
using Core.Storage;
using Core.Events;
using Core.Import;
using Core.Reports;
using Cli.Notifications;

IOrderStore store = StoreFactory.Create(args);

Console.WriteLine($"Сховище: {store.GetType().Name}");

if (args.Contains("--file"))
{
  string dataPath = Path.Combine(
      Directory.GetCurrentDirectory(), "data", "orders.json");

  Console.WriteLine($"Файл з даними: {dataPath}");
}

var command = args.FirstOrDefault(
    a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
  case "report-top":
    foreach (var l in ReportService.TopCustomers(store.List(), 5))
    {
      Console.WriteLine(
          $"{l.CustomerId,-10} " +
          $"Замовлень: {l.OrdersCount,3} " +
          $"Сума: {l.Total,12:F2} грн");
    }
    break;

  case "report-expensive":
    foreach (var o in ReportService.ExpensiveOrders(store.List(), 5000m))
    {
      Console.WriteLine(
          $"{o.OrderId,-10} " +
          $"{o.CustomerId,-10} " +
          $"{o.Status,-12} " +
          $"{o.Total,12:F2} грн");
    }
    break;

  case "report-groups":
    foreach (var g in ReportService.ByStatus(store.List()))
    {
      Console.WriteLine(
          $"{g.Key,-12} " +
          $"Замовлень: {g.Count,3} " +
          $"Сума: {g.Total,12:F2} грн");
    }
    break;

  case "report-sum":
    var t = ReportService.Totals(store.List());

    Console.WriteLine(
        $"Замовлень: {t.Items}, " +
        $"Загальна сума: {t.Total:F2} грн, " +
        $"Середній чек: {t.Average:F2} грн, " +
        $"Максимальне: {t.Max:F2} грн");
    break;

  case "demo-deferred":
    DemoDeferred();
    break;

  case "demo-events":
    DemoEvents();
    break;

  default:
    Console.WriteLine(
        "Команди: report-top | report-expensive | " +
        "report-groups | report-sum | " +
        "demo-deferred | demo-events");
    break;
}

return 0;

void DemoDeferred()
{
  Console.WriteLine("\nВідкладене виконання LINQ");

  var result = ReportService.DeferredExecution(store.List());

  Console.WriteLine($"Кількість до змін: {result.Before}");
  Console.WriteLine($"Кількість після змін: {result.After}");
  Console.WriteLine($"У знімку після очищення: {result.Snapshot}");
}

void DemoEvents()
{
  string logPath = Path.Combine(
    Directory.GetCurrentDirectory(), "logs", "app.log");


  INotificationSink[] sinks =
  [
      new ConsoleSink(),
      new FileSink(logPath)
  ];

  var service = new OrderService(store, sinks);

  void OnOrderChanged(
      object? sender, OrderChangedEventArgs e)
  {
    Console.WriteLine($"Подія Changed: {e.Kind}");
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

  Console.WriteLine(
      $"Створено замовлення: {secondOrder.Id}");

  Console.WriteLine($"Подій отримано: {changes}");

  Console.WriteLine("\nПошук замовлень:");

  var expensive = service.FindAll(o => o.Total > 5000m);

  Console.WriteLine(
      $"Замовлень понад 5000 грн: {expensive.Count}");

  foreach (var order in expensive)
  {
    Console.WriteLine(
        $"ID: {order.Id}, " +
        $"Статус: {order.Status}, " +
        $"Сума: {order.Total}");
  }

  Console.WriteLine("\nІмпорт CSV з Action<string>");

  string csvPath = Path.Combine(
      Directory.GetCurrentDirectory(), "data", "sample.csv");

  int skipped = 0;

  var result = ProductCsvImporter.Load(
      csvPath,
      message =>
      {
        skipped++;
        Console.WriteLine($"Пропущено: {message}");
      });

  Console.WriteLine(
      $"Успішно імпортовано: {result.Items.Count}");

  Console.WriteLine(
      $"Пропущено рядків: {skipped}");
}