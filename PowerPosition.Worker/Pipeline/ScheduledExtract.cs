namespace PowerPosition.Worker.Pipeline;

/// <summary>
/// One unit of work on the extract queue: a single scheduled run of the report. Pure data — the
/// producer decides everything on it; the consumer only reads it.
/// </summary>
/// <remarks>
/// <see cref="TradingDate"/> is resolved at <em>enqueue</em> time, never recomputed on processing:
/// a tick that waits in the queue across midnight must still extract the day it was scheduled for.
/// </remarks>
/// <param name="ScheduledAtUtc">The instant the run was scheduled for. Drives the report's file-name timestamp.</param>
/// <param name="TradingDate">The day-ahead date to extract the position for.</param>
public sealed record ScheduledExtract(DateTimeOffset ScheduledAtUtc, DateOnly TradingDate);
