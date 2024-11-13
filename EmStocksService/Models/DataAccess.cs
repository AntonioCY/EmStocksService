using EmStocksService.Helper.Enums;

namespace EmStocksService.Models;

public class DataAccess
{
    public Settings GetSettings()
    {
        // As an imrovement in the current implementation the values for the Settings initialization should be moved to the apps
        // but do not see the sense of implementing it as this class is also replacable by the Repository in the DataAccess
        return new(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(150), TimeSpan.FromSeconds(60), 20);
    }

    public List<PricesStreamInfo> GetPricesStreamsInfo()
    {
        List<PricesStreamInfo> pricesStreamInfoList = [];

        foreach (DataStream dataStream in Enum.GetValues(typeof(DataStream)))
        {
            string displayName = Enum.GetName(typeof(DataStream), dataStream)!;
            PricesStreamInfo pricesStreamInfo = new(Guid.NewGuid().ToString(), displayName, (int)dataStream);
            pricesStreamInfoList.Add(pricesStreamInfo);
        }

        return pricesStreamInfoList;
    }
}