
namespace Core.Configuration;

public sealed class AppOptions
{
  public string EnvironmentName { get; set; } = "Production";
  public StorageOptions Storage { get; set; } = new();
  public LoggingOptions Logging { get; set; } = new();
  public OrdersOptions Orders { get; set; } = new();
}

public sealed class StorageOptions
{
  public string DataPath { get; set; } = "data";
  public string FileName { get; set; } = "orders.json";

  public string ResolveFullPath(string baseDir)
  {
    if (string.IsNullOrWhiteSpace(DataPath) ||
        string.IsNullOrWhiteSpace(FileName))
      throw new ArgumentException("Шлях і назва файлу даних обов'язкові.");

    if (Path.IsPathRooted(FileName) ||
        FileName != Path.GetFileName(FileName))
      throw new ArgumentException("FileName повинен бути лише назвою файлу.");

    var root = Path.IsPathRooted(DataPath)
        ? DataPath
        : Path.Combine(baseDir, DataPath);

    root = Path.GetFullPath(root);
    Directory.CreateDirectory(root);

    return Path.Combine(root, FileName);
  }
}

public sealed class LoggingOptions
{
  public string MinimumLevel { get; set; } = "Information";
}

public sealed class OrdersOptions
{
  public string Currency { get; set; } = "UAH";
}
