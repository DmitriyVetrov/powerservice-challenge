using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerPosition.Worker.Configuration;

namespace PowerPosition.Worker.Pipeline;

/// <summary>
/// Drives the schedule: enqueues one extract on start, then one per interval tick. It only writes to
/// the queue — the extract runs on <see cref="ExtractWorker"/>. Decoupling the timer from execution
/// means a tick is never dropped because the previous extract is still running.
/// </summary>
public sealed class ScheduleProducer(
    ChannelWriter<ScheduledExtract> writer,
    IOptions<ExtractOptions> options,
    TimeProvider timeProvider,
    ILogger<ScheduleProducer> logger) : BackgroundService
{
    private static readonly TimeZoneInfo London = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);
        logger.LogInformation("Schedule producer started; interval is {IntervalMinutes} minute(s).", options.Value.IntervalMinutes);

        try
        {
            // Created before the first enqueue so the first tick is anchored to start time
            // (start + interval), not to how long the immediate enqueue took.
            using var timer = new PeriodicTimer(interval, timeProvider);

            // Run an extract at startup, before the first interval elapses.
            await EnqueueAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await EnqueueAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown — fall through to completing the queue.
        }
        finally
        {
            // Signal the consumer that no more items are coming, so it can drain and stop.
            writer.Complete();
            logger.LogInformation("Schedule producer stopped; queue completed.");
        }
    }

    private async Task EnqueueAsync(CancellationToken ct)
    {
        var tradingDate = DayAheadTradingDate();
        var item = new ScheduledExtract(timeProvider.GetUtcNow(), tradingDate);

        // WriteAsync, never TryWrite: a full queue blocks the producer rather than dropping a tick.
        await writer.WriteAsync(item, ct);

        logger.LogInformation(
            "Enqueued extract for trading date {TradingDate}, scheduled at {ScheduledAtUtc:o}.",
            tradingDate, item.ScheduledAtUtc);
    }

    /// <summary>
    /// Day-ahead trading date: today in Europe/London plus one day. Resolved from the London wall
    /// clock, never UTC, so a run just after midnight UTC still targets the correct London day.
    /// </summary>
    private DateOnly DayAheadTradingDate()
    {
        var londonNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), London);
        return DateOnly.FromDateTime(londonNow.Date).AddDays(1);
    }
}
