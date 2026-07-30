using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerPosition.Web.Configuration;
using PowerPosition.Web.Reports;

namespace PowerPosition.Tests;

public sealed class ReportCatalogTests : IDisposable
{
    private const string SampleCsv = "Local Time,Volume\n23:00,100\n00:00,200\n";

    private readonly string _root;

    public ReportCatalogTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "PowerPositionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private ReportCatalog Catalog(string? outputPath = null) => new(
        Options.Create(new ReportOptions { OutputPath = outputPath ?? _root }),
        new CsvReportParser(),
        NullLogger<ReportCatalog>.Instance);

    private string Write(string name, DateTime lastWriteUtc)
    {
        var path = Path.Combine(_root, name);
        File.WriteAllText(path, SampleCsv);
        File.SetLastWriteTimeUtc(path, lastWriteUtc);
        return path;
    }

    // Newest first, by write time rather than by name — the list must not assume a naming scheme.
    [Fact]
    public void List_ReturnsCsvFiles_NewestFirst()
    {
        Write("older.csv", new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc));
        Write("newer.csv", new DateTime(2026, 7, 16, 13, 0, 0, DateTimeKind.Utc));

        Assert.Equal(["newer.csv", "older.csv"], Catalog().List().Select(r => r.Name));
    }

    // The worker stages each extract as "{name}.{guid}.tmp" in this same folder before renaming it
    // into place, so a half-written file must never reach the list.
    [Fact]
    public void List_IgnoresNonCsvFiles()
    {
        Write("report.csv", new DateTime(2026, 7, 16, 12, 0, 0, DateTimeKind.Utc));
        File.WriteAllText(Path.Combine(_root, "report.csv.abc123.tmp"), SampleCsv);
        File.WriteAllText(Path.Combine(_root, "notes.txt"), "ignore me");

        Assert.Equal(["report.csv"], Catalog().List().Select(r => r.Name));
    }

    // Normal before the worker's first run: an empty list, not an exception.
    [Fact]
    public void List_ReturnsEmpty_WhenTheFolderDoesNotExist()
    {
        var catalog = Catalog(Path.Combine(_root, "not-created-yet"));

        Assert.Empty(catalog.List());
    }

    [Fact]
    public void RootPath_IsAbsolute_EvenWhenConfiguredRelatively()
    {
        Assert.True(Path.IsPathRooted(Catalog("some/relative/path").RootPath));
    }

    [Fact]
    public async Task LoadAsync_ParsesTheRequestedReport()
    {
        Write("report.csv", DateTime.UtcNow);

        var report = await Catalog().LoadAsync("report.csv", CancellationToken.None);

        Assert.Equal("report.csv", report.FileName);
        Assert.Equal(2, report.Timestamps.Count);
    }

    // The name comes from the browser, so it is untrusted: a traversal must not escape the folder.
    [Fact]
    public async Task LoadAsync_RejectsPathTraversal()
    {
        var outside = Path.Combine(_root, "..", "escaped.csv");
        File.WriteAllText(outside, SampleCsv);

        try
        {
            var catalog = Catalog();

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => catalog.LoadAsync("../escaped.csv", CancellationToken.None));
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => catalog.LoadAsync(outside, CancellationToken.None));
        }
        finally
        {
            File.Delete(outside);
        }
    }

    [Fact]
    public async Task LoadAsync_Throws_WhenTheReportDoesNotExist()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => Catalog().LoadAsync("missing.csv", CancellationToken.None));
    }
}
