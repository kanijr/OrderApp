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

  ## Запуск
  ```bash
  dotnet build
  dotnet run --project src/Cli
  ```

  ## Публікація

  ```bash
  dotnet publish src/Cli -c Release -r win-x64 --self-contained true
  dotnet publish src/Cli -c Release -r win-x64 --self-contained false
  dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
  ```

  ## Середовище
  .NET SDK 10.0, Windows 10 x64


  ## Розміри публікацій

  | RID | Режим | Розмір | Потрібен встановлений runtime |
  |---|---|---:|---|
  | win-x64 | self-contained | 76.5 МБ | ні |
  | win-x64 | framework-dependent | 195 КБ | так (.NET 10) |
  | linux-x64 | self-contained | 78.8 МБ | ні |