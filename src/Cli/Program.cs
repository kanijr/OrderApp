using Core.Dto;
using Core.Import;
using Core.Domain;

if (args.Contains("--domain"))
{
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
  Console.WriteLine($"Статус: {order.Status}");

  OrderDto dto = order.ToDto();
  Order restored = Order.FromDto(dto);

  Console.WriteLine($"Відновлене замовлення: {restored.Id}");
  Console.WriteLine($"Сума після відновлення: {restored.Total} грн");


  Console.WriteLine("\n=== Сценарій 2: порушення інваріантів ===");

  TryDo("Порожній покупець", () => Order.Create(" "));
  TryDo("Нульова кількість", () => OrderLine.Create("P-003", "Клавіатура", 1000m, 0));
  TryDo("Від'ємна ціна", () => OrderLine.Create("P-003", "Клавіатура", -100m, 1));
  TryDo("Додавання після підтвердження", () => order.AddLine("P-003", "Клавіатура", 1000m, 1));
  TryDo("Повторне підтвердження", () => order.Confirm());

  Console.WriteLine($"\nСума після помилок: {order.Total} грн");
  Console.WriteLine($"Кількість рядків після помилок: {order.Lines.Count}");

  Order emptyOrder = Order.Create("C-002");
  TryDo("Підтвердження порожнього замовлення", () => emptyOrder.Confirm());


  Console.WriteLine("\n=== Додаткове 1: імпорт у домен ===");

  List<OrderLineDto> testDtos =
  [
    new("P-001", "Ноутбук", 25000m, 1),
  new("P-002", "Мишка", 500m, 2),
  new("P-003", "Клавіатура", -100m, 1),
  new("P-004", "Монітор", 6000m, 0)
  ];

  ImportResult<OrderLine> domainResult = DomainImporter.ToDomain(testDtos);

  Console.WriteLine(
    $"Статистика: всього записів {domainResult.Total}, " +
    $"прийнято {domainResult.Accepted}, " +
    $"пропущено {domainResult.Skipped}, " +
    $"помилок {domainResult.ErrorPercent:F1}%");

  foreach (OrderLine line in domainResult.Items)
  {
    Console.WriteLine(
      $" {line.ProductId,-7} {line.Name,-30} " +
      $"{line.Price,10:F2} грн x {line.Quantity}");
  }

  foreach (string error in domainResult.Errors)
  {
    Console.WriteLine($" ! {error}");
  }


  Console.WriteLine("\n=== Додаткове 2: ліміт замовлень ===");

  Customer buyer = Customer.Create("C-010", "Іван Петренко");
  List<Order> buyerOrders = [];

  for (int i = 1; i <= 5; i++)
  {
    Order newOrder = CustomerOrderService.CreateOrder(buyer, buyerOrders);
    newOrder.AddLine("P-001", "Ноутбук", 25000m, 1);

    buyerOrders.Add(newOrder);
    Console.WriteLine($"Замовлення {i}: {newOrder.Status}");
  }

  TryDo("Створення шостого замовлення",
    () => buyerOrders.Add(CustomerOrderService.CreateOrder(buyer, buyerOrders)));

  buyerOrders[0].Confirm();
  Console.WriteLine($"Перше замовлення після підтвердження: {buyerOrders[0].Status}");

  Order sixthOrder = CustomerOrderService.CreateOrder(buyer, buyerOrders);
  buyerOrders.Add(sixthOrder);
  Console.WriteLine($"Нове замовлення після підтвердження: {sixthOrder.Status}");

  buyerOrders[1].Cancel();
  Console.WriteLine($"Друге замовлення після скасування: {buyerOrders[1].Status}");

  Order seventhOrder = CustomerOrderService.CreateOrder(buyer, buyerOrders);
  buyerOrders.Add(seventhOrder);

  Console.WriteLine($"Нове замовлення після скасування: {seventhOrder.Status}");
  Console.WriteLine($"Кількість чернеток: {buyerOrders.Count(o => o.Status == OrderStatus.Draft)}");


  Console.WriteLine("\n=== Додаткове 3: стани замовлення ===");

  Order cancelledOrder = Order.Create("C-003");
  cancelledOrder.AddLine("P-004", "Монітор", 6000m, 1);

  Console.WriteLine($"Початковий статус: {cancelledOrder.Status}");

  cancelledOrder.Cancel();

  Console.WriteLine($"Після скасування: {cancelledOrder.Status}");

  TryDo("Підтвердження скасованого замовлення", () => cancelledOrder.Confirm());
  TryDo("Повторне скасування", () => cancelledOrder.Cancel());
  TryDo("Скасування підтвердженого замовлення", () => order.Cancel());
  TryDo("Додавання товару до скасованого", () => cancelledOrder.AddLine("P-005", "Кабель", 200m, 1));


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

}
else
{
  string path = args.FirstOrDefault(arg => !arg.StartsWith("--")) ??
       Path.Combine("data", "sample.csv");

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
}


return 0;
