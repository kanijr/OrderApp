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
    └── src/
        ├── Core/
        │   ├── Core.csproj
        │   └── EnvironmentInfo.cs
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

```bash
dotnet build
dotnet run --project src/Cli
```

Для виведення інформації у форматі JSON:

```bash
dotnet run --project src/Cli -- --json
```

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

## Додаткові завдання

### Single-file публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true
```

Результат:

- розмір публікації: 70 мб;
- кількість файлів у каталозі publish: 3.

### Trimmed публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishTrimmed=true
```

Розмір trimmed-публікації: 19,3 мб.