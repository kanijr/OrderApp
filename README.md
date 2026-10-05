# OrderApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення.

Сутності: Customer, Product, Order, OrderLine.

Призначення: оформлення замовлень і підрахунку їхніх сум.

## Структура solution


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
        │   ├── Dto/
        │   │   ├── ProductDto.cs
        │   │   ├── CustomerDto.cs
        │   │   ├── MixedItemDto.cs
        │   │   └── ImportResult.cs
        │   └── Import/
        │       ├── ProductCsvImporter.cs
        │       ├── ProductJsonImporter.cs
        │       └── MixedCsvImporter.cs
        └── Cli/
            ├── Cli.csproj
            └── Program.cs

### Multi-targeting

Бібліотека Core збирається для двох цільових фреймворків:

- `net8.0`;
- `net10.0`.

## Середовище

.NET SDK 10.0, Windows 10 x64.

## Запуск

Збірка проєкту:

```bash
dotnet build
dotnet run --project src/Cli
dotnet run --project src/Cli -- data/sample.csv
dotnet run --project src/Cli -- data/sample.json
dotnet run --project src/Cli -- data/mixed.csv --mixed
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

### JSON

Файл `data/sample.json` містить товари у форматі:

```json
{
  "id": "P-001",
  "name": "Ноутбук Lenovo",
  "price": 29999.99
}
```

```json
{
  "id": "P-002",
  "name": "Миша Logitech",
  "price": 799.50
}
```

### Mixed CSV

Файл `data/mixed.csv` містить товари й покупців:

```text
P;P-001;Ноутбук Lenovo;29999.99
C;C-001;Іван Петренко;ivan@example.com
```

де:

- `P` — `Product`;
- `C` — `Customer`.
## Публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true
```

## Розміри публікацій

| RID | Режим | Розмір | Потрібен встановлений runtime |
|---|---|---:|---|
| win-x64 | self-contained | 76.5 МБ | ні |
| win-x64 | framework-dependent | 196 КБ | так (.NET 10) |
| linux-x64 | self-contained | 78.8 МБ | ні |
