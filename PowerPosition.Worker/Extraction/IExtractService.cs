using PowerPosition.Worker.Pipeline;

namespace PowerPosition.Worker.Extraction;

/// <summary>
/// Runs one scheduled extract end to end: fetch trades for the item's trading date, aggregate them,
/// and write the CSV report. Owns no timer and no queue; those belong to the caller.
/// </summary>
public interface IExtractService
{
    /// <summary>
    /// Executes <paramref name="extract"/> and returns the full path of the report written.
    /// </summary>
    /// <param name="extract">The scheduled item. Its <see cref="ScheduledExtract.TradingDate"/> is
    /// the day fetched; the trading date is never recomputed here.</param>
    /// <param name="ct">Cancels the run.</param>
    /// <returns>The full path of the CSV file that was written.</returns>
    /// <remarks>
    /// A run that fails after its retries are exhausted logs the failure and propagates, for the
    /// caller to skip.
    /// </remarks>
    Task<string> RunAsync(ScheduledExtract extract, CancellationToken ct);
}
