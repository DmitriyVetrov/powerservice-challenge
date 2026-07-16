using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerPosition.Worker.Configuration;
using PowerPosition.Worker.Domain;
using PowerPosition.Worker.Reporting;

namespace PowerPosition.Tests;

public sealed class CsvPositionWriterTests : IDisposable
{
    // A July instant: Europe/London is on BST (+1), so the local wall clock is an hour ahead of
    // UTC. That +1 is what this test pivots on.
    private static readonly DateTimeOffset JulyExtractUtc = new(2025, 7, 15, 10, 30, 0, TimeSpan.Zero);

    private static readonly PositionRow[] SampleRows =
    [
        new(new TimeOnly(23, 0), 150),
        new(new TimeOnly(0, 0), 80),
    ];

    private readonly string _root;
    private readonly string _outputPath;

    public CsvPositionWriterTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "PowerPositionTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        // Deliberately NOT created: the writer must create the output directory itself.
        _outputPath = Path.Combine(_root, "output");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private CsvPositionWriter Writer() => new(Options.Create(new ExtractOptions { OutputPath = _outputPath }), NullLogger<CsvPositionWriter>.Instance);

    // The file name derives from London local time, not UTC. 10:30 UTC in July is
    // 11:30 BST — the name must show 1130, proving the +1 offset was applied.
    [Fact]
    public async Task FileName_UsesLondonLocalTime_NotUtc()
    {
        var path = await Writer().WriteAsync(SampleRows, JulyExtractUtc, CancellationToken.None);

        Assert.Equal("PowerPosition_20250715_1130.csv", Path.GetFileName(path));
    }
}
