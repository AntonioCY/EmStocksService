using EmStocksService.Interfaces;
using EmStocksService.Models;
using EmStocksService.Services;
using Serilog;

namespace EmStocksService;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<DataAccess>();
        builder.Services.AddSingleton<IEmStocksService, StockPriceService>();
        builder.Services.AddHostedService<StocksServiceWorker>();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var host = builder.Build();
            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "EmStockservice has failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
