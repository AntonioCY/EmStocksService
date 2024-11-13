namespace EmStocksService.Models;

public class Settings(TimeSpan publishInterval, TimeSpan maxUnchangedPeriod, TimeSpan streamSwitchDelay, decimal priceChangeThresholdPercentage)
{
    public int PublishIntervalMs { get; } = (int)publishInterval.TotalMilliseconds;
    public int MaxUnchangedPeriodMs { get; } = (int)maxUnchangedPeriod.TotalMilliseconds;
    public int StreamSwitchDelayThresholdMs { get; } = (int)streamSwitchDelay.TotalMilliseconds;
    public decimal PriceChangeThresholdPercentage { get; } = priceChangeThresholdPercentage;
}
