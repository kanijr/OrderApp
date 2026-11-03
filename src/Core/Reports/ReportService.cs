using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
  public static IReadOnlyList<CustomerTotalLine> TopCustomers(
      IEnumerable<Order> source, int n) =>
      source
          .GroupBy(o => o.CustomerId)
          .Select(g => new CustomerTotalLine(
              g.Key,
              g.Count(),
              g.Sum(o => o.Total)))
          .OrderByDescending(x => x.Total)
          .ThenBy(x => x.CustomerId, StringComparer.Ordinal)
          .Take(n)
          .ToList();

  public static IReadOnlyList<OrderReportLine> ExpensiveOrders(
      IEnumerable<Order> source, decimal threshold) =>
      source
          .Where(o => o.Total > threshold)
          .OrderByDescending(o => o.Total)
          .ThenBy(o => o.Id, StringComparer.Ordinal)
          .Select(o => new OrderReportLine(
              o.Id,
              o.CustomerId,
              o.Status.ToString(),
              o.Total))
          .ToList();

  public static IReadOnlyList<GroupSummary> ByStatus(
      IEnumerable<Order> source) =>
      source
          .GroupBy(o => o.Status)
          .Select(g => new GroupSummary(
              g.Key.ToString(),
              g.Count(),
              g.Sum(o => o.Total)))
          .OrderByDescending(g => g.Total)
          .ThenBy(g => g.Key, StringComparer.Ordinal)
          .ToList();

  public static (int Items, decimal Total,
      decimal Average, decimal Max) Totals(
      IEnumerable<Order> source)
  {
    var list = source.ToList();

    return list.Count == 0
        ? (0, 0m, 0m, 0m)
        : (
            list.Count,
            list.Sum(o => o.Total),
            list.Average(o => o.Total),
            list.Max(o => o.Total)
        );
  }


  public static (int Before, int After, int Snapshot)
      DeferredExecution(IEnumerable<Order> orders)
  {
    var source = orders.ToList();

    if (source.Count == 0)
      return (0, 0, 0);

    var query = source.Where(o => o.Total >= 0m);

    int before = query.Count();

    source.Add(source[0]);

    int after = query.Count();

    var snapshot = query.ToList();

    source.Clear();

    return (before, after, snapshot.Count);
  }
}