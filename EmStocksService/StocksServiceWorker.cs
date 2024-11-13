using EmStocksService.Interfaces;

namespace EmStocksService;

public class StocksServiceWorker : IHostedService
{
    private readonly IEmStocksService _emStocksService;

    public StocksServiceWorker(IEmStocksService emStocksService)
    {
        _emStocksService = emStocksService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Start prices processing in EmStocksService
        await _emStocksService.StartProcessingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _emStocksService.StopService();
        // Stop once completed
        return Task.CompletedTask;
    }
}
