using PowerPosition.Web.Models;

namespace PowerPosition.Web.Reports;

/// <summary>
/// Turns the text of an extract CSV into the model the chart binds to.
/// </summary>
public interface ICsvReportParser
{
    /// <summary>Parses <paramref name="content"/>. Throws <see cref="FormatException"/> if it is not a report CSV.</summary>
    /// <param name="fileName">Name to record on the result; not used for parsing.</param>
    /// <param name="content">Full file text.</param>
    ReportData Parse(string fileName, string content);
}
