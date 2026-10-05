namespace Core.Dto;

public sealed record ImportResult<T>(
    IReadOnlyList<T> Items,
    IReadOnlyList<string> Errors)
{
    public int Accepted => Items.Count;

    public int Skipped => Errors.Count;

    public int Total => Accepted + Skipped;

    public double ErrorPercent =>
        Total == 0
            ? 0
            : Skipped * 100.0 / Total;
}