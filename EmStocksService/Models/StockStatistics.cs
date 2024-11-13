namespace EmStocksService.Models;

public class StockStatistics
{
    public decimal MaxPrice { get; private set; } = decimal.MinValue;
    public decimal MinPrice { get; private set; } = decimal.MaxValue;
    public decimal MaxFluctuation { get; private set; }

    public void Update(decimal price)
    {
        MaxPrice = Math.Max(MaxPrice, price);
        MinPrice = Math.Min(MinPrice, price);
        MaxFluctuation = Math.Max(MaxFluctuation, Math.Abs(price - MinPrice));
    }
}
