namespace EmStocksService.Models;

/// <summary>
/// There should be one StreamPricesReceiver instance per each available prices stream and for each 
/// StreamPricesReceiver instance there is a SEPARATE THREAD which listens to the corresponding prices
/// stream and it calls the RaisePriceReceived method whenever a new price is received.
/// Assume that there is implementation in place for the functionality that listens to the corresponding prices
/// stream and whenever a price for a stock is received it calls the RaisePriceReceived method.     
/// Assume that the information about the available price streams are in the database, in a table with
/// a field of type string that identifies the prices stream and that the code to read the stream name identifiers 
/// from the database is already implemented.
/// </summary>
public class StreamPricesReceiver(string streamId, int publishInterval)
{
    private readonly string _streamId = streamId;
    private readonly int _publishInterval = publishInterval;
    private static readonly Dictionary<string, StreamPricesReceiver> instances = [];
    public event EventHandler<PriceReceivedEventArgs>? PriceReceived;

    public static StreamPricesReceiver GetInstance(string streamId, int publishInterval)
    {
        if (!instances.TryGetValue(streamId, out StreamPricesReceiver? value))
        {
            value = new StreamPricesReceiver(streamId, publishInterval);
            instances[streamId] = value;
        }

        return value;
    }

    public void RaisePriceReceived(string streamId, string stockName, decimal price, DateTime priceTime)
    {
        PriceReceived?.Invoke(this, new PriceReceivedEventArgs(streamId, stockName, price, priceTime));
    }

    /// <summary>
    /// This method will create the subscription to the prices stream so whenever a price is available
    /// the RaisePriceReceived method will be called. This method must be called during the service 
    /// initialization for each StreamPricesReceiver instance that will be created.
    /// Assume that the implementation of this method is in place.
    /// </summary>
    /// <param name="streamId"></param>
    public void Subscribe(string streamId, List<string> symbols)
    {
        // Probably it make sense to discuss adding the the symbols(stock names) list in Subscription as a parameter because for the
        // subscription it is commonly required part
        // Also, it should be set the delay in the end of the loop like the following "await Task.Delay(_publishInterval);"
        // to broadcast prices updates according to the _publishInterval limit that value is taken from the Settings.PublishIntervalMs
        // the event will be rised with the help of RaisePriceRecieved method
    }

    /// <summary>
    /// This method will remove the subscription from the the prices stream and 
    /// must be called during the service termination for each StreamPricesReceiver 
    /// instance that has been instantiated so as to release the held resources.
    /// Assume that the implementation of this method is in place.
    /// </summary>
    /// <param name="streamId"></param>
    public void Unsubscribe(string streamId)
    {
        //..........................
    }

    public string GetStreamId()
    {
        return _streamId;
    }
}
