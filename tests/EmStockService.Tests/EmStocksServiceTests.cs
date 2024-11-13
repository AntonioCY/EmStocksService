using EmStocksService.Interfaces;
using EmStocksService.Models;
using EmStocksService.Services;
using Moq;

namespace EmStockService.Tests;

public class EmStocksServiceTests
{
    private readonly DataAccess _dataAccess;
    private readonly Settings _settings;
    private readonly Mock<PricesPublisher> _pricesPublisherMock;
    private readonly StockPriceService _service;
    private readonly string _streamId;
    public EmStocksServiceTests()
    {
        _dataAccess = new DataAccess();
        _settings = _dataAccess.GetSettings();
        _pricesPublisherMock = new Mock<PricesPublisher>();
        _streamId = _dataAccess.GetPricesStreamsInfo().First().StreamId;
        _service = new StockPriceService(_dataAccess);
    }

    [Fact]
    public async Task StartProcessingAsync_ShouldStartProcessing()
    {
        var cancellationToken = new CancellationToken();
        var emStocksService = new Mock<IEmStocksService>();

        await emStocksService.Object.StartProcessingAsync(cancellationToken);

        emStocksService.Verify(x => x.StartProcessingAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public void InitializeStreamReceivers_ShouldSubscribeToStreams()
    {
        var receivers = _service.GetStreamReceivers();

        Assert.True(receivers.ContainsKey("PrimarySource"));
        Assert.True(receivers.ContainsKey("SecondarySource"));
        Assert.True(receivers.ContainsKey("ThirdSource"));

        foreach (var receiverList in receivers.Values)
        {
            foreach (var receiver in receiverList)
            {
                Assert.NotNull(receiver);
            }
        }
    }

    [Fact]
    public async Task ProcessPriceAsync_ShouldPublishPriceWhenConditionsMet()
    {
        string stockName = "StockA";
        decimal newPrice = 105;
        _service.OnPricePublished += (name, price) =>
        {
            Assert.Equal(stockName, name);
            Assert.Equal(newPrice, price);
        };

        await _service.ProcessPriceAsync(stockName, newPrice);
    }

    [Fact]
    public void HandlePriceReceived_ShouldInvokeProcessPriceAsync()
    {
        var receiver = new StreamPricesReceiver(_streamId, _settings.PublishIntervalMs);
        bool isProcessPriceCalled = false;
        receiver.PriceReceived += (sender, args) =>
        {
            isProcessPriceCalled = true;
        };

        receiver.RaisePriceReceived("stream1", "StockA", 105, DateTime.UtcNow);

        Assert.True(isProcessPriceCalled);
    }

    [Fact]
    public async Task StartListening_ShouldStartListeningOnAllReceivers()
    {
        await _service.StartProcessingAsync(CancellationToken.None);

        foreach (var stream in _service.GetStreamReceivers().Values)
        {
            foreach (var receiver in stream)
            {
                Assert.NotNull(receiver);
            }
        }
    }


    [Fact]
    public void StopService_ShouldUnsubscribeFromAllReceivers()
    {
        _service.StopService();

        foreach (var stream in _service.GetStreamReceivers().Values)
        {
            foreach (var receiver in stream)
            {
                Assert.NotNull(receiver);
            }
        }
    }

    [Fact]
    public void HandlePriceReceived_Should_ProcessPriceAsync()
    {
        var stockName = "AAPL";
        var price = 100;

        _service.HandlePriceReceived(null, new PriceReceivedEventArgs(_streamId, stockName, price, DateTime.UtcNow));
    }

    [Fact]
    public async Task Should_NotPublishPrice_When_PriceUnchangedWithinThreshold()
    {
        decimal publishedTimes = 0;
        _service.OnPricePublished += (stock, price) => publishedTimes++;

        await _service.ProcessPriceAsync("AAPL", 150m);
        await _service.ProcessPriceAsync("AAPL", 150m); // second processing no price change

        Assert.Equal(1, publishedTimes); // should be 1 publish
    }

    [Fact]
    public async Task Should_UpdateStatistics_When_NewPriceIsProcessed()
    {
        await _service.ProcessPriceAsync("AAPL", 150m); // first publication
        await _service.ProcessPriceAsync("AAPL", 190m); // price change

        var statistics = await _service.GetStatisticsAsync("AAPL");

        Assert.Equal(150m, statistics.MinPrice);
        Assert.Equal(190m, statistics.MaxPrice);
        Assert.Equal(40m, statistics.MaxFluctuation);
    }
}
