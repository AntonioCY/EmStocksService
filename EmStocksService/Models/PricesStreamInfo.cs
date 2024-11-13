namespace EmStocksService.Models;

public class PricesStreamInfo(string streamId, string stockName, int preferenceOrder)
{
    public string StreamId { get; } = streamId;
    public string StockName { get; } = stockName;
    public int PreferenceOrder { get; } = preferenceOrder;

    public override string ToString()
    {
        return $"{nameof(StreamId)}: {StreamId}, {nameof(StockName)}: {StockName}, {nameof(PreferenceOrder)}: {PreferenceOrder}";
    }
}
