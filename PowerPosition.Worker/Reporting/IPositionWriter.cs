using PowerPosition.Worker.Domain;

namespace PowerPosition.Worker.Reporting;

/// <summary>
/// Persists an aggregated day-ahead position report. The implementation decides the format, file
/// name and directory; callers supply only the rows and the instant the extract was taken.
/// </summary>
public interface IPositionWriter
{
    /// <summary>
    /// Writes <paramref name="rows"/> as a report file and returns the full path written.
    /// </summary>
    /// <param name="rows">The aggregated position, one row per trading period, in period order.</param>
    /// <param name="extractedAtUtc">
    /// The instant the extract was taken. The file name derives from it in Europe/London local
    /// time — never UTC, never the machine locale.
    /// </param>
    /// <param name="ct">Cancels the write.</param>
    /// <returns>The full path of the file that was written.</returns>
    Task<string> WriteAsync(IReadOnlyList<PositionRow> rows, DateTimeOffset extractedAtUtc, CancellationToken ct);
}
