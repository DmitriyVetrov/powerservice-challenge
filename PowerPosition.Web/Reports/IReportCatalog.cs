using PowerPosition.Web.Models;

namespace PowerPosition.Web.Reports;

/// <summary>
/// Discovers and loads the CSV reports in the configured extract folder.
/// </summary>
public interface IReportCatalog
{
    /// <summary>The folder being scanned, as an absolute path. Shown to the user when it holds no reports.</summary>
    string RootPath { get; }

    /// <summary>Lists the available reports, newest first. Empty if the folder does not exist or holds no CSVs.</summary>
    IReadOnlyList<ReportFile> List();

    /// <summary>Reads and parses one report by file name.</summary>
    /// <exception cref="FileNotFoundException">The name does not resolve to a file inside the extract folder.</exception>
    /// <exception cref="FormatException">The file is not a valid report CSV.</exception>
    Task<ReportData> LoadAsync(string name, CancellationToken ct);
}
