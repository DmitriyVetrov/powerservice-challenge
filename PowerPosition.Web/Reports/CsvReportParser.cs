using System.Globalization;
using PowerPosition.Web.Models;

namespace PowerPosition.Web.Reports;

/// <summary>
/// Reads the extract CSV written by the worker: a time column followed by one or more value columns.
/// </summary>
/// <remarks>
/// The counterpart to the worker's <c>CsvPositionWriter</c>, but deliberately more permissive than
/// that writer is strict. Nothing here is tied to the "Volume" column or to the file name: column 0
/// is the time axis and every column after it becomes a named series, so a wider CSV plots as
/// multiple lines without a code change.
///
/// The CSV only stores wall-clock "HH:mm", and a day-ahead trading day runs 23:00 → 22:00, so the
/// times wrap past midnight. See <see cref="BuildTimestamps"/> for how the real time axis is rebuilt.
/// </remarks>
public sealed class CsvReportParser : ICsvReportParser
{
    /// <summary>
    /// Date the first row is pinned to. Arbitrary and never displayed — only the time-of-day part of
    /// a timestamp is ever rendered — but fixed rather than <c>DateTime.Today</c> so that parsing the
    /// same file twice always gives the same answer.
    /// </summary>
    private static readonly DateTime Anchor = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>The writer emits "HH:mm"; the others are accepted so a hand-edited file still loads.</summary>
    private static readonly string[] TimeFormats = ["HH:mm", "H:mm", "HH:mm:ss"];

    public ReportData Parse(string fileName, string content)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);

        // Split on '\n' and trim '\r' rather than splitting on Environment.NewLine: the writer always
        // emits '\n', but a file that has been through a Windows editor should still load.
        var lines = content.Split('\n');

        var headerIndex = Array.FindIndex(lines, static line => !string.IsNullOrWhiteSpace(line));
        if (headerIndex < 0)
        {
            throw new FormatException("The report is empty.");
        }

        var seriesNames = ParseHeader(lines[headerIndex], headerIndex + 1);

        var times = new List<TimeOnly>();
        var columns = new List<double?>[seriesNames.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            columns[i] = [];
        }

        for (var i = headerIndex + 1; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue; // trailing newline, or a blank line someone left behind
            }

            var cells = line.Split(',');

            if (!TimeOnly.TryParseExact(cells[0].Trim(), TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                throw new FormatException($"Line {i + 1}: '{cells[0].Trim()}' is not a time of day.");
            }

            times.Add(time);

            for (var c = 0; c < columns.Length; c++)
            {
                // A short row, an empty cell or a non-numeric cell becomes a gap in that one series
                // rather than a failed load — one bad cell should not cost you the whole report.
                var cellIndex = c + 1;
                var value = cellIndex < cells.Length
                    && double.TryParse(cells[cellIndex].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                        ? parsed
                        : (double?)null;

                columns[c].Add(value);
            }
        }

        if (times.Count == 0)
        {
            throw new FormatException("The report has a header but no data rows.");
        }

        var series = new ReportSeries[seriesNames.Length];
        for (var i = 0; i < series.Length; i++)
        {
            series[i] = new ReportSeries(seriesNames[i], columns[i]);
        }

        return new ReportData(fileName, BuildTimestamps(times), series);
    }

    private static string[] ParseHeader(string headerLine, int lineNumber)
    {
        var cells = headerLine.TrimEnd('\r').Split(',');
        if (cells.Length < 2)
        {
            throw new FormatException($"Line {lineNumber}: the header needs a time column and at least one value column.");
        }

        // cells[0] is the time column's own header ("Local Time"); its name is never displayed.
        var names = new string[cells.Length - 1];
        for (var i = 0; i < names.Length; i++)
        {
            var name = cells[i + 1].Trim();
            names[i] = name.Length > 0 ? name : $"Series {i + 1}";
        }

        return names;
    }

    /// <summary>
    /// Rebuilds a real time axis from times-of-day that wrap past midnight.
    /// </summary>
    /// <remarks>
    /// Each row sits on the same date as the row before it, unless its time went <em>backwards</em>,
    /// which can only mean the day rolled over (23:00 → 00:00). The comparison is strict, so the hour
    /// that repeats on a DST fall-back day (01:00 twice) stays on the same date and produces two
    /// points at the same instant — which is exactly what the source file says. Nothing here assumes
    /// the rows are one hour apart.
    /// </remarks>
    private static DateTime[] BuildTimestamps(List<TimeOnly> times)
    {
        var timestamps = new DateTime[times.Count];
        var date = Anchor.Date;

        timestamps[0] = date + times[0].ToTimeSpan();

        for (var i = 1; i < times.Count; i++)
        {
            var candidate = date + times[i].ToTimeSpan();
            if (candidate < timestamps[i - 1])
            {
                date = date.AddDays(1);
                candidate = date + times[i].ToTimeSpan();
            }

            timestamps[i] = candidate;
        }

        return timestamps;
    }
}
