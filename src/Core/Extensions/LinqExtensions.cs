namespace Core.Extensions;

public static class LinqExtensions
{
  public static IEnumerable<T> TopBy<T, TKey>(
      this IEnumerable<T> source,
      Func<T, decimal> selector,
      Func<T, TKey> tieBreaker,
      int n)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(selector);
    ArgumentNullException.ThrowIfNull(tieBreaker);
    ArgumentOutOfRangeException.ThrowIfNegative(n);

    return source
        .OrderByDescending(selector)
        .ThenBy(tieBreaker)
        .Take(n);
  }
}