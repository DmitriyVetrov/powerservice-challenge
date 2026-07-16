using Axpo;
using Microsoft.Extensions.Logging;
using PowerPosition.Worker.Domain;
using PowerPosition.Worker.Reporting;
using PowerPosition.Worker.Pipeline;

namespace PowerPosition.Worker.Extraction;

/// <summary>
/// Composes the domain into one callable unit: <see cref="IPowerService.GetTradesAsync"/> →
/// <see cref="PositionAggregator.Aggregate"/> → <see cref="IPositionWriter.WriteAsync"/>.
/// Only the fetch is retried; PowerService throws intermittently by design.
/// </summary>
public sealed class ExtractService(
    IPowerService powerService,
    PositionAggregator aggregator,
    IPositionWriter writer,
    TimeProvider timeProvider,
    ILogger<ExtractService> logger) : IExtractService
{
    // 3 attempts with the delay doubling each time: waits of 2s then 4s.
    private const int MaxAttempts = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(2);

    public async Task<string> RunAsync(ScheduledExtract extract, CancellationToken ct)
    {
        var tradingDate = extract.TradingDate;

        logger.LogInformation(
            "Starting extract for trading date {TradingDate}, scheduled at {ScheduledAtUtc:o}.",
            tradingDate, extract.ScheduledAtUtc);

        var startedAt = timeProvider.GetTimestamp();

        // The item's trading date, never a recomputed "today": a tick that waited in the queue
        // across midnight still extracts the day it was scheduled for.
        var tradeDate = tradingDate.ToDateTime(TimeOnly.MinValue);
        var trades = await FetchTradesWithRetryAsync(tradeDate, tradingDate, ct);

        var rows = aggregator.Aggregate(trades, tradingDate);

        // File-name timestamp is the instant the run was scheduled for, not now.
        var path = await writer.WriteAsync(rows, extract.ScheduledAtUtc, ct);

        var elapsedMs = timeProvider.GetElapsedTime(startedAt).TotalMilliseconds;
        logger.LogInformation(
            "Completed extract for trading date {TradingDate}: wrote {Path} in {ElapsedMs:F0} ms.",
            tradingDate, path, elapsedMs);
        return path;
    }

    /// <summary>
    /// Fetches trades, retrying transient failures with exponential backoff. Retried attempts log a
    /// warning; the final failure logs one error and propagates for the caller to skip.
    /// </summary>
    private async Task<IEnumerable<PowerTrade>> FetchTradesWithRetryAsync(
        DateTime tradeDate, DateOnly tradingDate, CancellationToken ct)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await powerService.GetTradesAsync(tradeDate);
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                // Reason but no stack trace — that is saved for the final error.
                var delay = BaseDelay * Math.Pow(2, attempt - 1);
                logger.LogWarning(
                    "Fetch of trades for trading date {TradingDate} failed on attempt {Attempt} of {MaxAttempts}; retrying in {DelaySeconds}s. Reason: {FailureReason}",
                    tradingDate, attempt, MaxAttempts, delay.TotalSeconds, ex.Message);

                await Task.Delay(delay, timeProvider, ct);
            }
            catch (Exception ex)
            {
                // Log once with the exception and propagate; ExtractWorker skips the item, so a
                // permanently failing extract never blocks the queue.
                logger.LogError(ex,
                    "Fetch of trades for trading date {TradingDate} failed after {MaxAttempts} attempts; giving up.",
                    tradingDate, MaxAttempts);
                throw;
            }
        }
    }
}
