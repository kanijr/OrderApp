# OrderApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення.

Сутності: Customer, Product, Order, OrderLine.

Призначення: оформлення замовлень і підрахунку їхніх сум.

### Профілі

| Профіль | Рівень логування | Каталог даних |
|---|---|---|
| Production | Information | data |
| Development | Debug | data |

Відносний каталог даних визначається від `AppContext.BaseDirectory`, а не від поточного робочого каталогу.

### Ключі конфігурації

| Ключ | Змінна середовища | Значення за замовчуванням |
|---|---|---|
| EnvironmentName | CROSSAPP_EnvironmentName | Production |
| Storage:DataPath | CROSSAPP_Storage__DataPath | data |
| Storage:FileName | CROSSAPP_Storage__FileName | orders.json |
| Logging:MinimumLevel | CROSSAPP_Logging__MinimumLevel | Information |
| Orders:Currency | CROSSAPP_Orders__Currency | UAH |
| Integration:ApiKey | CROSSAPP_Integration__ApiKey | Не задано |

### Приклади PowerShell

```powershell
$env:CROSSAPP_ENVIRONMENT = "Development"
$env:CROSSAPP_Storage__DataPath = "my-data"
$env:CROSSAPP_DATA_PATH = "my-data"
dotnet run --project src/Cli -f net10.0 -- export
```

### Приклади Bash

```bash
export CROSSAPP_ENVIRONMENT=Development
export CROSSAPP_Storage__DataPath=my-data
export CROSSAPP_DATA_PATH=my-data
dotnet run --project src/Cli -f net10.0 -- export
```

`CROSSAPP_DATA_PATH` має окремий пріоритет над `Storage:DataPath`.

### Імпорт та експорт

```bash
dotnet run --project src/Cli -f net10.0 -- export
dotnet run --project src/Cli -f net10.0 -- import
dotnet run --project src/Cli -f net10.0 -- import --path backup.json
```

### User Secrets

```bash
dotnet user-secrets init --project src/Cli
dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli
dotnet user-secrets list --project src/Cli
```

### Інваріанти доменої моделі

- Ідентифікатори замовлення, покупця й товару не можуть бути порожніми.
- Ім'я покупця та назва товару обов'язкові; email покупця необов'язковий.
- Ціна товару не може бути від'ємною, а кількість повинна бути більшою за нуль.
- Додавати товари можна лише до замовлення зі статусом `Draft`.
- Порожнє замовлення не можна підтвердити.
- Дозволені переходи станів: `Draft → Confirmed` і `Draft → Cancelled`. Повторні та інші переходи заборонені.
- Один покупець може мати не більше п'яти замовлень у статусі `Draft` одночасно. Після підтвердження або скасування чернетки можна створити нову через `OrderService`.


## Структура solution

```text
OrderApp/
├── OrderApp.slnx
├── README.md
├── .gitignore
├── tests/
│   └── Core.Tests/
│       ├── Core.Tests.csproj
│       ├── ImportTests.cs
│       ├── OrderServiceTests.cs
│       ├── TestData.cs
│       └── Abstractions/
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── DomainException.cs
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs
    │   ├── Abstractions/
    │   │   ├── INotificationSink.cs
    │   │   ├── IOrderLineImporter.cs
    │   │   └── IOrderStore.cs
    │   ├── Abstractions/
    │   │   └── AppOptions.cs
    │   ├── Dto/
    │   │   ├── ProductDto.cs
    │   │   ├── CustomerDto.cs
    │   │   ├── MixedItemDto.cs
    │   │   ├── OrderDto.cs
    │   │   ├── OrderLineDto.cs
    │   │   └── ImportResult.cs
    │   ├── Domain/
    │   │   ├── Customer.cs
    │   │   ├── Order.cs
    │   │   ├── OrderLine.cs
    │   │   └── OrderStatus.cs
    │   ├── Events/
    │   │   ├── OrderChangedEvenArgs.cs
    │   │   └── OrderChangeKind.cs
    │   ├── Extensions/
    │   │   └── LinqExtensions.cs
    │   ├── Import/
    │   │   ├── ProductCsvImporter.cs
    │   │   ├── ProductJsonImporter.cs
    │   │   ├── MixedCsvImporter.cs
    │   │   └── DomainImporter.cs
    │   ├── Reports/
    │   │   ├── BenchmarkService.cs
    │   │   ├── CustomerOrderSummary.cs
    │   │   ├── CustomerTotalLine.cs
    │   │   ├── GroupSummary.cs
    │   │   ├── OrderReportLine.cs
    │   │   └── ReportService.cs
    │   ├── Services/
    │   │   └── OrderService.cs
    │   └── Storage/
    │       ├── CachingOrderStore.cs
    │       ├── FileOrderStore.cs
    │       ├── InMemoryOrderStore.cs
    │       ├── JsonOrderLineImporter.cs
    │       ├── OrderJsonSerializer.cs
    │       └── StoreFactory.cs
    └── Cli/
        ├── appsettings.Development.json
        ├── appsettings.json
        ├── Cli.csproj
        ├── Program.cs
        └── Notifications/
            ├── ConsoleSink.cs
            └── FileSink.cs
```

### Multi-targeting

Бібліотека Core збирається для двох цільових фреймворків:

- `net8.0`;
- `net10.0`.

## Середовище

.NET SDK 10.0, Windows 10 x64.

## Запуск

Збірка проєкту та приклади запуску (доступні команди: `report-top, report-expensive, report-sum, demo-deferred,demo-events, report-customers, report-top-orders, report-groups-query, report-benchmark`):

```bash
dotnet build
dotnet run --project src/Cli -- report-top
```


## Формати даних

### CSV

Файл `data/sample.csv` містить товари у форматі:

```text
P-001;Ноутбук Lenovo;29999.99
P-002;Миша Logitech;799.50
```

Формат полів:

```text
id;name;price
```

## Публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true
```