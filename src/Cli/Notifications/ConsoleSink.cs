
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
  public void Notify(OrderChangedEventArgs e)
  {
    Console.WriteLine(
        $"{e.At:HH:mm:ss} {e.Kind,-15} " +
        $"{e.OrderId} Рядків: {e.LinesCount} " +
        $"Сума: {e.Total:F2} грн");
  }
}
