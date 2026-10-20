# OrderApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення.

Сутності: Customer, Product, Order, OrderLine.

Призначення: оформлення замовлень і підрахунку їхніх сум.


### Інваріанти доменої моделі

- Ідентифікатори замовлення, покупця й товару не можуть бути порожніми.
- Ім'я покупця та назва товару обов'язкові; email покупця необов'язковий.
- Ціна товару не може бути від'ємною, а кількість повинна бути більшою за нуль.
- Додавати товари можна лише до замовлення зі статусом `Draft`.
- Порожнє замовлення не можна підтвердити.
- Дозволені переходи станів: `Draft → Confirmed` і `Draft → Cancelled`. Повторні та інші переходи заборонені.
- Один покупець може мати не більше п'яти замовлень у статусі `Draft` одночасно. Після підтвердження або скасування чернетки можна створити нову через `CustomerOrderService`.


## Структура solution

```text
OrderApp/
├── OrderApp.slnx
├── README.md
├── .gitignore
├── data/
│   ├── sample.csv
│   ├── sample.json
│   └── mixed.csv
└── src/
    ├── Core/
    │   ├── Core.csproj
    │   ├── EnvironmentInfo.cs
    │   ├── SampleData.cs
    │   ├── Abstractions/
    │   │   └── IOrderStore.cs
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
    │   ├── Import/
    │   │   ├── ProductCsvImporter.cs
    │   │   ├── ProductJsonImporter.cs
    │   │   ├── MixedCsvImporter.cs
    │   │   └── DomainImporter.cs
    │   ├── Services/
    │   │   └── OrderService.cs
    │   └── Storage/
    │       ├── CachingOrderStore.cs
    │       ├── FileOrderStore.cs
    │       ├── InMemoryOrderStore.cs
    │       └── StoreFactory.cs
    └── Cli/
        ├── Cli.csproj
        └── Program.cs
```

### Multi-targeting

Бібліотека Core збирається для двох цільових фреймворків:

- `net8.0`;
- `net10.0`.

## Середовище

.NET SDK 10.0, Windows 10 x64.

## Запуск

Збірка проєкту та приклади запуску:

```bash
dotnet build
dotnet run --project src/Cli
dotnet run --project src/Cli -- --file
dotnet run --project src/Cli -- --cache
dotnet run --project src/Cli -- --file --cache
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