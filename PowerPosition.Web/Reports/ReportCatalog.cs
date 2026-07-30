using Microsoft.Extensions.Options;
using PowerPosition.Web.Configuration;
using PowerPosition.Web.Models;

namespace PowerPosition.Web.Reports;

/// <summary>
/// Scans <c>Extract:OutputPath</c> for the worker's CSV extracts and loads them on demand.
/// </summary>
/// <remarks>
/// The only file-system aware part of the report pipeline — parsing is pure, so the pages depend on
/// this and never on <see cref="File"/> directly.
///
/// The folder is shared with a running worker, which stages each extract as
/// <c>{name}.{guid}.tmp</c> and then renames it into place. Filtering on "*.csv" therefore never
/// picks up a half-written file, and the rename is atomic, so a read either sees the old file or the
/// whole new one.
/// </remarks>
public sealed class ReportCatalog : IReportCatalog
{
    private readonly ICsvReportParser parser;
    private readonly ILogger<ReportCatalog> logger;

    public ReportCatalog(IOptions<ReportOptions> options, ICsvReportParser parser, ILogger<ReportCatalog> logger)
    {
        this.parser = parser;
        this.logger = logger;

        // Resolved once, up front: every later path check compares against this, and a relative
        // setting must not silently change meaning if the process's current directory ever moves.
        RootPath = Path.GetFullPath(options.Value.OutputPath);
    }

    public string RootPath { get; }

    public IReadOnlyList<ReportFile> List()
    {
        if (!Directory.Exists(RootPath))
        {
            // Normal before the worker's first run — the page shows an empty list, not an error.
            logger.LogDebug("Extract folder {Path} does not exist yet.", RootPath);
            return [];
        }

        return new DirectoryInfo(RootPath)
            .EnumerateFiles("*.csv", SearchOption.TopDirectoryOnly)
            .Select(file => new ReportFile(file.Name, new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero)))
            // Newest first by write time, not by name: the list must not depend on the file naming scheme.
            .OrderByDescending(report => report.ModifiedUtc)
            .ThenBy(report => report.Name, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<ReportData> LoadAsync(string name, CancellationToken ct)
    {
        var path = ResolveInsideRoot(name);
        var content = await File.ReadAllTextAsync(path, ct);

        return parser.Parse(Path.GetFileName(path), content);
    }

    /// <summary>
    /// Maps a requested file name to a full path, refusing anything that would escape the extract folder.
    /// </summary>
    /// <remarks>
    /// The name arrives from the browser, so it is untrusted. <see cref="Path.GetFileName(string)"/>
    /// strips any directory part, and the containment check afterwards is the belt-and-braces guard
    /// against a name that still resolves elsewhere (a symlinked folder, an absolute path on Windows).
    /// </remarks>
    private string ResolveInsideRoot(string name)
    {
        var fileName = Path.GetFileName(name);
        if (string.IsNullOrEmpty(fileName))
        {
            throw new FileNotFoundException($"'{name}' is not a report file name.", name);
        }

        var fullPath = Path.GetFullPath(Path.Combine(RootPath, fileName));
        var root = RootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(root, StringComparison.Ordinal) || !File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Report '{fileName}' was not found in the extract folder.", fileName);
        }

        return fullPath;
    }
}
