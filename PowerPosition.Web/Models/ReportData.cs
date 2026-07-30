namespace PowerPosition.Web.Models;

/// <summary>
/// One plotted line: a column of the CSV, named after its header.
/// </summary>
/// <param name="Name">Column header, e.g. "Volume". Used as the trace name in the chart legend.</param>
/// <param name="Values">One value per timestamp, positionally aligned with <see cref="ReportData.Timestamps"/>.
/// <c>null</c> marks a cell that was empty or unparseable, which plotly renders as a gap.</param>
public sealed record ReportSeries(string Name, IReadOnlyList<double?> Values);

/// <summary>
/// A parsed CSV report: a shared time axis plus one or more value series.
/// </summary>
/// <remarks>
/// Today's extracts have a single "Volume" column, but nothing here is specific to it — the parser
/// turns every column after the time column into a <see cref="ReportSeries"/> and the chart draws one
/// trace per series, so a wider CSV needs no code change.
/// </remarks>
/// <param name="FileName">Name of the file this was read from, for the chart title.</param>
/// <param name="Timestamps">Wall-clock instants reconstructed from the CSV's "HH:mm" labels.
/// <see cref="DateTimeKind.Unspecified"/> on purpose: the CSV carries no offset, and an offset-free
/// value is what plotly.js parses most reliably.</param>
/// <param name="Series">One entry per value column, in column order.</param>
public sealed record ReportData(
    string FileName,
    IReadOnlyList<DateTime> Timestamps,
    IReadOnlyList<ReportSeries> Series);
