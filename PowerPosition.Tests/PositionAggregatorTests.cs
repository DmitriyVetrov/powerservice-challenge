using Axpo;
using Microsoft.Extensions.Logging.Abstractions;
using PowerPosition.Worker.Domain;

namespace PowerPosition.Tests;

public class PositionAggregatorTests
{
    private readonly TradingDayCalendar _calendar = new();

    private PositionAggregator Aggregator() =>
        new(_calendar, NullLogger<PositionAggregator>.Instance);

    /// <summary>Builds a PowerTrade whose period 1..N volumes are the supplied values in order.</summary>
    private static PowerTrade Trade(DateOnly date, params double[] volumesByPeriod)
    {
        var trade = PowerTrade.Create(date.ToDateTime(TimeOnly.MinValue), volumesByPeriod.Length);
        for (var i = 0; i < volumesByPeriod.Length; i++)
        {
            trade.Periods[i].SetVolume(volumesByPeriod[i]);
        }

        return trade;
    }

    private static double[] Repeat(double value, int count) =>
        Enumerable.Repeat(value, count).ToArray();

    // The worked example from docs/requirements.md, asserted row-for-row against
    // the expected table (an independent oracle, not re-derived from the calendar).
    [Fact]
    public void WorkedExample_MatchesRequirementsTableExactly()
    {
        var tradingDate = new DateOnly(2015, 1, 4); // "1/4/2015" (US format) — an ordinary 24-period day.

        // Trade 1: 100 across all 24 periods.
        var trade1 = Trade(tradingDate, Repeat(100d, 24));
        // Trade 2: 50 for periods 1..11, then -20 for periods 12..24.
        var trade2 = Trade(tradingDate, [.. Repeat(50d, 11), .. Repeat(-20d, 13)]);

        var expected = new PositionRow[]
        {
            new(new TimeOnly(23, 0), 150),
            new(new TimeOnly(0, 0), 150),
            new(new TimeOnly(1, 0), 150),
            new(new TimeOnly(2, 0), 150),
            new(new TimeOnly(3, 0), 150),
            new(new TimeOnly(4, 0), 150),
            new(new TimeOnly(5, 0), 150),
            new(new TimeOnly(6, 0), 150),
            new(new TimeOnly(7, 0), 150),
            new(new TimeOnly(8, 0), 150),
            new(new TimeOnly(9, 0), 150),
            new(new TimeOnly(10, 0), 80),
            new(new TimeOnly(11, 0), 80),
            new(new TimeOnly(12, 0), 80),
            new(new TimeOnly(13, 0), 80),
            new(new TimeOnly(14, 0), 80),
            new(new TimeOnly(15, 0), 80),
            new(new TimeOnly(16, 0), 80),
            new(new TimeOnly(17, 0), 80),
            new(new TimeOnly(18, 0), 80),
            new(new TimeOnly(19, 0), 80),
            new(new TimeOnly(20, 0), 80),
            new(new TimeOnly(21, 0), 80),
            new(new TimeOnly(22, 0), 80),
        };

        var rows = Aggregator().Aggregate([trade1, trade2], tradingDate);

        Assert.Equal(expected, rows);
    }
}
