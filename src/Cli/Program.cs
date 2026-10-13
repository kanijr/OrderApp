using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("OrderApp – інформація про середовище");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС : {report.OsDescription}");
Console.WriteLine($"Runtime : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
Console.WriteLine($"Каталог : {report.BaseDirectory}");
Console.WriteLine($"Збірка: {report.BuildNote}");
Console.WriteLine("");

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
  Console.WriteLine(
      $"Файл не знайдено: {Path.GetFullPath(path)}");

  return 1;
}

if (args.Contains("--mixed"))
{
  ImportResult<MixedItemDto> result =
      MixedCsvImporter.Load(path);

  Console.WriteLine(
    $"Статистика: всього записів {result.Total}, " +
    $"прийнято {result.Accepted}, " +
    $"пропущено {result.Skipped}, " +
    $"помилок {result.ErrorPercent:F1}%");

  foreach (MixedItemDto item in result.Items.Take(5))
  {
    switch (item)
    {
      case ProductItemDto product:
        Console.WriteLine(
            $" Product  " +
            $"{product.Product.Id,-7} " +
            $"{product.Product.Name,-30} " +
            $"{product.Product.Price,10:F2}");
        break;

      case CustomerItemDto customer:
        Console.WriteLine(
            $" Customer " +
            $"{customer.Customer.Id,-7} " +
            $"{customer.Customer.Name,-30} " +
            $"{customer.Customer.Email ?? "-"}");
        break;
    }
  }

  if (result.Errors.Count > 0)
  {
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
      Console.WriteLine($" ! {error}");
    }
  }
}
else
{
  string extension = Path.GetExtension(path).ToLowerInvariant();

  ImportResult<ProductDto> productResult = extension switch
  {
    ".csv" => ProductCsvImporter.Load(path),

    ".json" => ProductJsonImporter.Load(path),

    _ => new ImportResult<ProductDto>(
        [],
        [$"Непідтримуваний формат файлу: {extension}"])
  };

  Console.WriteLine(
    $"Статистика: всього записів {productResult.Total}, " +
    $"прийнято {productResult.Accepted}, " +
    $"пропущено {productResult.Skipped}, " +
    $"помилок {productResult.ErrorPercent:F1}%");

  foreach (ProductDto product in productResult.Items.Take(5))
  {
    Console.WriteLine(
        $" {product.Id,-7} " +
        $"{product.Name,-30} " +
        $"{product.Price,10:F2}");
  }

  if (productResult.Errors.Count > 0)
  {
    Console.WriteLine(
        $"Пропущено рядків: {productResult.Errors.Count}");

    foreach (string error in productResult.Errors)
    {
      Console.WriteLine($" ! {error}");
    }
  }
}

Console.WriteLine("\n=== Сценарій 1: успішне замовлення ===");

Order order = Order.Create("C-001");

order.AddLine("P-001", "Ноутбук", 25000m, 1);
order.AddLine("P-002", "Мишка", 500m, 2);

Console.WriteLine($"Замовлення: {order.Id}");
Console.WriteLine($"Покупець: {order.CustomerId}");

foreach (OrderLine line in order.Lines)
{
  Console.WriteLine(
    $"{line.Name}: {line.Quantity} x {line.Price} = {line.Total} грн");
}

Console.WriteLine($"Загальна сума: {order.Total} грн");

order.Confirm();
Console.WriteLine($"Підтверджено: {order.IsConfirmed}");

OrderDto dto = order.ToDto();
Order restored = Order.FromDto(dto);

Console.WriteLine($"Відновлене замовлення: {restored.Id}");
Console.WriteLine($"Сума після відновлення: {restored.Total} грн");


Console.WriteLine("\n=== Сценарій 2: порушення інваріантів ===");

TryDo("Порожній покупець", () => Order.Create(" "));

TryDo("Нульова кількість", () => order.AddLine("P-003", "Клавіатура", 1000m, 0));

TryDo("Від'ємна ціна", () => OrderLine.Create("P-003", "Клавіатура", -100m, 1));

TryDo("Додавання після підтвердження", () => order.AddLine("P-003", "Клавіатура", 1000m, 1));

TryDo("Повторне підтвердження", () => order.Confirm());

Console.WriteLine($"\nСума після помилок: {order.Total} грн");
Console.WriteLine($"Кількість рядків після помилок: {order.Lines.Count}");

Order emptyOrder = Order.Create("C-002");

TryDo("Підтвердження порожнього замовлення", () => emptyOrder.Confirm());


return 0;


static void TryDo(string title, Action action)
{
  try
  {
    action();
    Console.WriteLine($"{title}: виняток не спрацював!");
  }
  catch (ArgumentException ex)
  {
    Console.WriteLine(
      $"{title}: {ex.GetType().Name} — {ex.Message}");
  }
  catch (InvalidOperationException ex)
  {
    Console.WriteLine(
      $"{title}: {ex.GetType().Name} — {ex.Message}");
  }
}
