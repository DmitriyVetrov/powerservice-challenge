namespace PowerPosition.Worker.Domain;

/// <summary>
/// One hourly period of a trading day.
/// </summary>
/// <param name="Period">1-based period number; period 1 is the first hour (23:00 local on D-1).</param>
/// <param name="StartUtc">The UTC instant at which the period starts.</param>
/// <param name="LocalTime">The Europe/London wall-clock time at <paramref name="StartUtc"/>.</param>
public sealed record TradingPeriod(int Period, DateTimeOffset StartUtc, TimeOnly LocalTime);
