using EmStocksService.Helper.Enums.Constants;
using EmStocksService.Interfaces;
using EmStocksService.Models;
using System.Collections.Concurrent;
using Serilog;

namespace EmStocksService.Services;

public class StockPriceService : IEmStocksService
{
    private readonly Settings _settings;
    private readonly DataAccess _dataAccess;
    private readonly ConcurrentDictionary<string, List<PricesStreamInfo>> _stockDataSources;
    private readonly Dictionary<string, List<StreamPricesReceiver>> _streamReceivers;
    private readonly ConcurrentDictionary<string, StockStatistics> _stockStatistics;
    private readonly ConcurrentDictionary<string, decimal> _lastPublishedPrices;
    private readonly Dictionary<string, DateTime> _lastPublishedTimes;
    private readonly Dictionary<string, DateTime> _lastUpdateTimes;
    private int _currentPreferenceOrder = 0;
    private readonly SemaphoreSlim _processPriceSemaphore = new(1);

    public event Action<string, decimal>? OnPricePublished;

    public StockPriceService(DataAccess dataAccess) 
    { 
    
        _dataAccess = dataAccess;
        _settings = _dataAccess.GetSettings();
        _stockStatistics = new ConcurrentDictionary<string, StockStatistics>();
        _lastPublishedPrices = new ConcurrentDictionary<string, decimal>();
        _lastPublishedTimes = [];
        _lastUpdateTimes = [];

        _streamReceivers = InitializeStreamReceivers();
        _stockDataSources = InitializeStockDataSources();
    }
    private ConcurrentDictionary<string, List<PricesStreamInfo>> InitializeStockDataSources()
    {
        var stockDataSources = new ConcurrentDictionary<string, List<PricesStreamInfo>?>();

        var dataStreamsList = _dataAccess.GetPricesStreamsInfo();
        InstrumentsConstants.Symbols.ForEach(symbol =>
        {
            stockDataSources.AddOrUpdate(symbol, [], (key, value) => dataStreamsList);
        });

        return stockDataSources!;
    }
    private Dictionary<string, List<StreamPricesReceiver>> InitializeStreamReceivers()
    {
        var streamsInfo = _dataAccess.GetPricesStreamsInfo();
        var groupedStreams = streamsInfo
            .GroupBy(info => info.StockName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(info => info.PreferenceOrder)
                    .Select(info => StreamPricesReceiver.GetInstance(info.StreamId, _settings.PublishIntervalMs))
                    .ToList()
            );

        // Subscribing on the prices changes initiated by the RaisePriceReceived from the StreamPricesReceiver
        // _settings.PublishIntervalMs is used to set the period of publishing the prices, used above
        foreach (var streamList in groupedStreams.Values)
        {
            foreach (var receiver in streamList)
            {
                receiver.PriceReceived += HandlePriceReceived;
                receiver.Subscribe(receiver.GetStreamId(), InstrumentsConstants.Symbols);
            }
        }

        return groupedStreams;
    }

    public void HandlePriceReceived(object? sender, PriceReceivedEventArgs e)
    {
        Task.Run(async () => await GetPriceFromPrimarySourceAsync(e.StockName, e.StreamId, e.Price));
    }
    private async Task<decimal> GetPriceFromPrimarySourceAsync(string stockSymbol, string streamId, decimal newPrice)
    {
        var sources = _stockDataSources[stockSymbol];
        var maxOrder = sources.Max(source => source.PreferenceOrder);

        foreach (var source in sources)
        {
            try
            {
                if (source.StreamId == streamId && source.PreferenceOrder == _currentPreferenceOrder)
                {
                    if (DateTime.UtcNow - _lastUpdateTimes.GetValueOrDefault(stockSymbol) <
                        TimeSpan.FromMilliseconds(_settings.StreamSwitchDelayThresholdMs))

                    {
                        await ProcessPriceAsync(stockSymbol, newPrice);                        
                    }
                    else
                    {
                        _currentPreferenceOrder = _currentPreferenceOrder == maxOrder ? 0 : _currentPreferenceOrder + 1;
                    }

                    return newPrice; // return price from the stream if there was no delay
                }
            }
            catch(Exception ex) 
            {
                Log.Error("Error - GetPriceFromPrimarySourceAsync", ex);
            }
        }

        // return the latest value or default
        return _lastPublishedPrices.GetValueOrDefault(stockSymbol, 0);
    }

    public async Task ProcessPriceAsync(string stockSymbol, decimal newPrice)
    {
        await _processPriceSemaphore.WaitAsync();

        try
        {
            var lastPublishedPrice = _lastPublishedPrices.GetValueOrDefault(stockSymbol, decimal.MinValue);
            var lastPublishedTime = _lastPublishedTimes.GetValueOrDefault(stockSymbol, DateTime.MinValue);
            var diffTimeInMilliseconds = (DateTime.UtcNow - lastPublishedTime).TotalMilliseconds;

            var priceChanged = lastPublishedPrice == decimal.MinValue || Math.Abs(newPrice - lastPublishedPrice)*100 / lastPublishedPrice >= _settings.PriceChangeThresholdPercentage;
            var timeElapsed = lastPublishedTime == DateTime.MinValue || diffTimeInMilliseconds >= _settings.PublishIntervalMs;
            var maxUnchangedPeriodMs = lastPublishedTime == DateTime.MinValue || diffTimeInMilliseconds <= _settings.MaxUnchangedPeriodMs;

            Log.Information($"timeElapsed={diffTimeInMilliseconds}");

            if ((priceChanged || timeElapsed) && !(!priceChanged && maxUnchangedPeriodMs))
            {
                _lastPublishedPrices[stockSymbol] = newPrice;
                _lastPublishedTimes[stockSymbol] = DateTime.UtcNow;

                _stockStatistics.GetOrAdd(stockSymbol, new StockStatistics()).Update(newPrice);
                PricesPublisher.Instance.Publish(stockSymbol, newPrice);   // Publish price using the single instance
                OnPricePublished?.Invoke(stockSymbol, newPrice);

                Log.Information($"Price = {newPrice} for the symbol = {stockSymbol} is published");
            }
        }
        finally
        {
            _processPriceSemaphore.Release();
        }
    }

    public Task StartProcessingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task<StockStatistics> GetStatisticsAsync(string stockSymbol)
    {
        return await Task.FromResult(_stockStatistics.GetValueOrDefault(stockSymbol)!);
    }

    public void StopService()
    {
        foreach (var streamList in _streamReceivers.Values)
        {
            foreach (var receiver in streamList)
            {
                receiver.Unsubscribe(receiver.GetStreamId());
            }
        }
    }

    public Dictionary<string, List<StreamPricesReceiver>> GetStreamReceivers()
    {
        return _streamReceivers;
    }
}

