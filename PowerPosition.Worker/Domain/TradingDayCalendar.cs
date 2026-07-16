namespace PowerPosition.Worker.Domain;

/// <summary>
/// Enumerates the hourly periods of a day-ahead trading day in the Europe/London time zone.
/// </summary>
public sealed class TradingDayCalendar
{
    private static readonly TimeZoneInfo London = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    /// <summary>
    /// Returns the ordered periods of the trading day for <paramref name="tradingDate"/>.
    /// The day runs from 23:00 local on D-1 to 23:00 local on D.
    /// </summary>
    /// <remarks>
    /// Steps in 1-hour UTC increments between the two boundary instants, converting each back to
    /// local for its wall-clock time. Stepping in UTC — never adding hours to a local clock — is
    /// what makes DST days fall out correctly at 23, 24 or 25 periods.
    /// </remarks>
    public IReadOnlyList<TradingPeriod> GetPeriods(DateOnly tradingDate)
    {
        // Europe/London transitions at 01:00/02:00, so 23:00 is never in a DST gap or
        // ambiguity — both boundaries are unambiguous instants.
        var startUtc = ToUtcInstant(tradingDate.AddDays(-1));
        var endUtc = ToUtcInstant(tradingDate);

        var periods = new List<TradingPeriod>();
        var period = 1;
        for (var instant = startUtc; instant < endUtc; instant = instant.AddHours(1))
        {
            var localTime = TimeOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, London).DateTime);
            periods.Add(new TradingPeriod(period, instant, localTime));
            period++;
        }

        return periods;
    }

    /// <summary>Converts 23:00 Europe/London local time on <paramref name="date"/> to its UTC instant.</summary>
    private static DateTimeOffset ToUtcInstant(DateOnly date)
    {
        var local = date.ToDateTime(new TimeOnly(23, 0), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, London), TimeSpan.Zero);
    }
}
