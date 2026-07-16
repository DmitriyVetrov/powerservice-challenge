namespace PowerPosition.Worker.Domain;

/// <summary>
/// One row of the aggregated day-ahead position report: the Europe/London
/// wall-clock time of a trading period and the total volume across all trades
/// for that period.
/// </summary>
/// <param name="LocalTime">The local start time of the period (e.g. 23:00 for period 1).</param>
/// <param name="Volume">Summed volume for the period. Matches <c>Axpo.PowerPeriod.Volume</c> (double).</param>
public sealed record PositionRow(TimeOnly LocalTime, double Volume);
