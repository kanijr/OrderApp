
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
  private readonly string _path;

  public FileSink(string path)
  {
    _path = path;
    Directory.CreateDirectory(
        Path.GetDirectoryName(_path)!);
  }

  public void Notify(OrderChangedEventArgs e)
  {
    string line =
        $"{e.At:HH:mm:ss} {e.Kind,-15} " +
        $"{e.OrderId} Рядків: {e.LinesCount} " +
        $"Сума: {e.Total:F2} грн";

    File.AppendAllText(
        _path, line + Environment.NewLine);
  }
}
