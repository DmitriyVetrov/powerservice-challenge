using PowerPosition.Web.Reports;

namespace PowerPosition.Tests;

public sealed class CsvReportParserTests
{
    private static readonly CsvReportParser Parser = new();

    // Trimmed from a real extract: the trading day opens at 23:00 and wraps past midnight.
    private const string SampleCsv =
        "Local Time,Volume\n" +
        "23:00,7167.56342765888\n" +
        "00:00,4999.73478108054\n" +
        "01:00,3788.339099537246\n";

    [Fact]
    public void Parse_ReadsEveryRow_AndNamesTheSeriesAfterItsColumn()
    {
        var report = Parser.Parse("PowerPosition_20260716_1302.csv", SampleCsv);

        Assert.Equal("PowerPosition_20260716_1302.csv", report.FileName);
        Assert.Equal(3, report.Timestamps.Count);

        var series = Assert.Single(report.Series);
        Assert.Equal("Volume", series.Name);
        Assert.Equal([7167.56342765888, 4999.73478108054, 3788.339099537246], series.Values);
    }

    // The CSV only stores "HH:mm", so 23:00 → 00:00 has to be read as crossing midnight rather
    // than as the clock jumping backwards. Everything downstream assumes an ascending axis.
    [Fact]
    public void Parse_RollsTheDateForward_WhenTheClockWrapsPastMidnight()
    {
        var report = Parser.Parse("report.csv", SampleCsv);

        Assert.Equal(new TimeSpan(1, 0, 0), report.Timestamps[1] - report.Timestamps[0]);
        Assert.Equal(report.Timestamps[0].Date.AddDays(1), report.Timestamps[1].Date);
        Assert.Equal(report.Timestamps[1].Date, report.Timestamps[2].Date);
    }

    // Nothing in the parser is tied to a single "Volume" column: the chart's multi-series support
    // rests on every column after the time column becoming its own series.
    [Fact]
    public void Parse_TurnsEveryValueColumn_IntoItsOwnSeries()
    {
        const string csv =
            "Local Time,Volume,Forecast\n" +
            "23:00,100,110\n" +
            "00:00,200,190\n";

        var report = Parser.Parse("wide.csv", csv);

        Assert.Equal(["Volume", "Forecast"], report.Series.Select(s => s.Name));
        Assert.Equal([100d, 200d], report.Series[0].Values);
        Assert.Equal([110d, 190d], report.Series[1].Values);
    }

    // On the autumn DST day the clock repeats 01:00. The comparison that detects a midnight wrap is
    // strict, so a repeated hour must not be mistaken for one — the row count has to survive intact.
    [Fact]
    public void Parse_KeepsBothRows_WhenAnHourRepeatsOnADstFallBackDay()
    {
        const string csv =
            "Local Time,Volume\n" +
            "00:00,10\n" +
            "01:00,20\n" +
            "01:00,30\n" +
            "02:00,40\n";

        var report = Parser.Parse("dst.csv", csv);

        Assert.Equal(4, report.Timestamps.Count);
        Assert.Equal(report.Timestamps[1], report.Timestamps[2]);
        Assert.Equal(report.Timestamps[0].Date, report.Timestamps[3].Date);
        Assert.Equal([10d, 20d, 30d, 40d], report.Series[0].Values);
    }

    // One unreadable cell is a gap in that series, not a failed report.
    [Fact]
    public void Parse_TreatsAnUnreadableValue_AsAGap()
    {
        var report = Parser.Parse("gap.csv", "Local Time,Volume\n23:00,100\n00:00,\n01:00,300\n");

        Assert.Equal([100d, null, 300d], report.Series[0].Values);
    }

    [Fact]
    public void Parse_HandlesWindowsLineEndings()
    {
        var report = Parser.Parse("crlf.csv", "Local Time,Volume\r\n23:00,100\r\n00:00,200\r\n");

        Assert.Equal(2, report.Timestamps.Count);
        Assert.Equal([100d, 200d], report.Series[0].Values);
    }

    [Fact]
    public void Parse_Throws_WhenATimeCellIsNotATime()
    {
        var ex = Assert.Throws<FormatException>(
            () => Parser.Parse("bad.csv", "Local Time,Volume\n23:00,100\nnot-a-time,200\n"));

        Assert.Contains("Line 3", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Parse_Throws_WhenThereIsNoValueColumn()
    {
        Assert.Throws<FormatException>(() => Parser.Parse("bad.csv", "Local Time\n23:00\n"));
    }

    [Fact]
    public void Parse_Throws_WhenTheFileHasNoDataRows()
    {
        Assert.Throws<FormatException>(() => Parser.Parse("empty.csv", "Local Time,Volume\n"));
    }

    // The writer formats with InvariantCulture; a comma-decimal machine must still read its output.
    [Fact]
    public void Parse_ReadsInvariantCultureNumbers_RegardlessOfTheCurrentCulture()
    {
        var original = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");

        try
        {
            var report = Parser.Parse("de.csv", "Local Time,Volume\n23:00,7167.56342765888\n");

            Assert.Equal(7167.56342765888, report.Series[0].Values[0]);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = original;
        }
    }
}
