using Serilog;

namespace EmStocksService.Models;

public class PricesPublisher
{
    private static readonly PricesPublisher instance = new();

    // Private constructor to prevent instantiation from outside the class
    private PricesPublisher()
    {
    }

    public static PricesPublisher Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// This method is to be called for publishing the selected price for each stock to
    /// the interested subscribers. Assume that the implementation is in place, there should 
    /// be a single PricesPublisher instance and that the Publish method is safe to be called
    /// simultaneously from multiple threads. 
    /// </summary>
    /// <param name="stockName"></param>
    /// <param name="price"></param>
    public void Publish(string stockName, decimal price)
    {
        Log.Information($"stockName={stockName}, price={price} published");
        //................. implementation logic goes here
    }
}
