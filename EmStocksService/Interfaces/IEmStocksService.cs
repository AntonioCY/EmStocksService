using EmStocksService.Models;

namespace EmStocksService.Interfaces;

public interface IEmStocksService
{
    Task StartProcessingAsync(CancellationToken cancellationToken);
    void StopService();
    event Action<string, decimal> OnPricePublished;
    Task ProcessPriceAsync(string stockSymbol, decimal newPrice);
    Task<StockStatistics> GetStatisticsAsync(string stockSymbol);
    Dictionary<string, List<StreamPricesReceiver>> GetStreamReceivers();
}
