using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PowerPosition.Worker.Extraction;

namespace PowerPosition.Worker.Pipeline;

/// <summary>
/// Consumes the extract queue, running each item through <see cref="IExtractService"/>. A single
/// reader, so extracts never overlap. A failing extract is logged and skipped rather than tearing
/// down the loop; on shutdown the loop drains what is already queued.
/// </summary>
public sealed class ExtractWorker(
    ChannelReader<ScheduledExtract> reader,
    IExtractService extractService,
    ILogger<ExtractWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Extract worker started; waiting for scheduled extracts.");

        try
        {
            await foreach (var extract in reader.ReadAllAsync(stoppingToken))
            {
                await ProcessAsync(extract, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown while waiting for the next item. Drain what is queued on an uncancelled
            // token: a queued extract must finish, not be aborted mid-flight.
            while (reader.TryRead(out var extract))
            {
                await ProcessAsync(extract, CancellationToken.None);
            }
        }

        logger.LogInformation("Extract worker stopped.");
    }

    private async Task ProcessAsync(ScheduledExtract extract, CancellationToken ct)
    {
        try
        {
            await extractService.RunAsync(extract, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutdown cancelled this extract — propagate to the drain path above rather than
            // logging it as a failure.
            throw;
        }
        catch (Exception ex)
        {
            // ExtractService already logged this with its exception; record only the skip.
            logger.LogWarning(
                "Extract for trading date {TradingDate} (scheduled at {ScheduledAtUtc:o}) failed and was skipped; continuing with the queue. Reason: {FailureReason}",
                extract.TradingDate, extract.ScheduledAtUtc, ex.Message);
        }
    }
}
