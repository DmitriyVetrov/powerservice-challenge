using Axpo;
using Microsoft.Extensions.Logging;

namespace PowerPosition.Worker.Domain;

/// <summary>
/// Aggregates <see cref="PowerTrade"/> volumes into one row per trading period, in period order.
/// The day's shape (23, 24 or 25 periods) comes from <see cref="TradingDayCalendar"/>, never assumed.
/// </summary>
public sealed class PositionAggregator(TradingDayCalendar calendar, ILogger<PositionAggregator> logger)
{
    /// <summary>
    /// Sums volume across <paramref name="trades"/> by period, returning one <see cref="PositionRow"/>
    /// per calendar period of <paramref name="tradingDate"/> in period order (23:00 first, 22:00 last
    /// on an ordinary day). Periods with no data are emitted as 0. A trade that disagrees with the
    /// calendar is logged as a warning rather than throwing.
    /// </summary>
    public IReadOnlyList<PositionRow> Aggregate(IEnumerable<PowerTrade> trades, DateOnly tradingDate)
    {
        var periods = calendar.GetPeriods(tradingDate);
        var expectedCount = periods.Count;

        // Keyed by 1-based period number; warns on anything the calendar has no slot for.
        var volumeByPeriod = new Dictionary<int, double>(expectedCount);
        foreach (var trade in trades)
        {
            if (trade.Periods.Length != expectedCount)
            {
                logger.LogWarning(
                    "Trade {TradeId} reports {ActualPeriods} periods but the calendar for {TradingDate} has {ExpectedPeriods}. Aggregating the periods that exist; missing calendar periods are filled with 0.",
                    trade.TradeId, trade.Periods.Length, tradingDate, expectedCount);
            }

            foreach (var period in trade.Periods)
            {
                if (period.Period < 1 || period.Period > expectedCount)
                {
                    logger.LogWarning(
                        "Trade {TradeId} contains period {Period}, outside the calendar range 1..{ExpectedPeriods} for {TradingDate}. Ignoring it.",
                        trade.TradeId, period.Period, expectedCount, tradingDate);
                    continue;
                }

                volumeByPeriod.TryGetValue(period.Period, out var running);
                volumeByPeriod[period.Period] = running + period.Volume;
            }
        }

        var rows = new List<PositionRow>(expectedCount);
        foreach (var period in periods)
        {
            volumeByPeriod.TryGetValue(period.Period, out var volume);
            rows.Add(new PositionRow(period.LocalTime, volume));
        }

        return rows;
    }
}
