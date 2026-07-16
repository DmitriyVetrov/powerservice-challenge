using System.Globalization;
using PowerPosition.Worker.Domain;

namespace PowerPosition.Tests;

public class TradingDayCalendarTests
{
    private readonly TradingDayCalendar _calendar = new();

    private static DateOnly D(string iso) =>
        DateOnly.ParseExact(iso, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    // Autumn back. The clocks fall 02:00 -> 01:00, so the day is 25 hours
    // long and local time 01:00 occurs twice at two distinct UTC instants an hour apart.
    [Fact]
    public void AutumnBack_20251026_Has25Periods_And0100AppearsTwiceOneHourApart()
    {
        var periods = _calendar.GetPeriods(D("2025-10-26"));

        Assert.Equal(25, periods.Count);

        var oneAm = periods.Where(p => p.LocalTime == new TimeOnly(1, 0)).ToList();
        Assert.Equal(2, oneAm.Count);
        Assert.NotEqual(oneAm[0].StartUtc, oneAm[1].StartUtc);
        Assert.Equal(TimeSpan.FromHours(1), oneAm[1].StartUtc - oneAm[0].StartUtc);
    }
}
