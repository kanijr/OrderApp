using System.Diagnostics;

namespace Core.Reports;

public static class BenchmarkService
{
  public static (
      int Items,
      double WithoutToListMs,
      double WithToListMs,
      int ResultCount) Compare(int count = 100_000)
  {
    var random = new Random(42);

    var source = Enumerable.Range(1, count)
        .Select(i => new OrderReportLine(
            $"O-{i:D6}",
            $"C-{i % 1000:D4}",
            "Confirmed",
            random.Next(100, 50_000)))
        .ToList();

    static List<OrderReportLine> WithoutToList(
        IEnumerable<OrderReportLine> source) =>
        source
            .Where(o => o.Total > 5000m)
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.OrderId)
            .ToList();

    static List<OrderReportLine> WithToList(
        IEnumerable<OrderReportLine> source) =>
        source
            .Where(o => o.Total > 5000m)
            .ToList()
            .OrderByDescending(o => o.Total)
            .ThenBy(o => o.OrderId)
            .ToList();

    WithoutToList(source);
    WithToList(source);

    const int repetitions = 10;

    double withoutMs = 0;
    double withMs = 0;

    var stopwatch = new Stopwatch();

    for (int i = 0; i < repetitions; i++)
    {
      if (i % 2 == 0)
      {
        withoutMs += Measure(
            () => WithoutToList(source), stopwatch);

        withMs += Measure(
            () => WithToList(source), stopwatch);
      }
      else
      {
        withMs += Measure(
            () => WithToList(source), stopwatch);

        withoutMs += Measure(
            () => WithoutToList(source), stopwatch);
      }
    }

    var resultWithout = WithoutToList(source);
    var resultWith = WithToList(source);

    if (!resultWithout.SequenceEqual(resultWith))
      throw new InvalidOperationException(
          "Результати LINQ-запитів не збігаються.");

    return (
        source.Count,
        withoutMs / repetitions,
        withMs / repetitions,
        resultWithout.Count
    );
  }

  private static double Measure(
      Func<List<OrderReportLine>> action,
      Stopwatch stopwatch)
  {
    stopwatch.Restart();

    var result = action();

    stopwatch.Stop();

    GC.KeepAlive(result);

    return stopwatch.Elapsed.TotalMilliseconds;
  }
}