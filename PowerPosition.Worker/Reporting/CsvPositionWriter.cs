using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerPosition.Worker.Configuration;
using PowerPosition.Worker.Domain;

namespace PowerPosition.Worker.Reporting;

/// <summary>
/// Writes the aggregated position to a CSV file.
/// </summary>
/// <remarks>
/// File name is <c>PowerPosition_yyyyMMdd_HHmm.csv</c>, timestamped from <c>extractedAtUtc</c> in
/// Europe/London local time. Values are formatted with <see cref="CultureInfo.InvariantCulture"/>,
/// so a comma-decimal culture cannot collide with the CSV separator.
///
/// Written to a temp file then <see cref="File.Move(string, string, bool)"/>d onto the final name —
/// an atomic same-directory rename, so a consumer never observes a partial CSV.
/// </remarks>
public sealed class CsvPositionWriter(IOptions<ExtractOptions> options, ILogger<CsvPositionWriter> logger) : IPositionWriter
{
    private static readonly TimeZoneInfo London = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private const string Header = "Local Time,Volume";

    public async Task<string> WriteAsync(IReadOnlyList<PositionRow> rows, DateTimeOffset extractedAtUtc, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var directory = options.Value.OutputPath;
        Directory.CreateDirectory(directory); // no-op if it already exists

        var fileName = FileNameFor(extractedAtUtc);
        var finalPath = Path.Combine(directory, fileName);

        // Unique name in the *same* directory: keeps the move an atomic same-volume rename and
        // stops two extracts in the same minute clashing on it.
        var tempPath = Path.Combine(directory, $"{fileName}.{Guid.NewGuid():N}.tmp");

        var content = BuildCsv(rows);

        try
        {
            await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            await using (var writer = new StreamWriter(stream, Utf8NoBom))
            {
                await writer.WriteAsync(content.AsMemory(), ct);
                await writer.FlushAsync(ct);
            }

            // Atomic replace: overwrite a same-minute re-run rather than fail on it.
            File.Move(tempPath, finalPath, overwrite: true);
        }
        finally
        {
            // The move consumed the temp file on success, so this only fires if it threw.
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch (IOException ex)
                {
                    logger.LogDebug(ex, "Could not remove temp file {TempPath} after a failed write.", tempPath);
                }
            }
        }

        logger.LogInformation("Wrote power position report to {Path} ({RowCount} rows).", finalPath, rows.Count);
        return finalPath;
    }

    private static string FileNameFor(DateTimeOffset extractedAtUtc)
    {
        var local = TimeZoneInfo.ConvertTime(extractedAtUtc, London);
        // Explicit InvariantCulture: an interpolated string would format with the current culture.
        return $"PowerPosition_{local.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture)}.csv";
    }

    private static string BuildCsv(IReadOnlyList<PositionRow> rows)
    {
        var sb = new StringBuilder();
        sb.Append(Header).Append('\n');
        foreach (var row in rows)
        {
            sb.Append(row.LocalTime.ToString("HH:mm", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(row.Volume.ToString(CultureInfo.InvariantCulture))
                .Append('\n');
        }

        return sb.ToString();
    }
}
