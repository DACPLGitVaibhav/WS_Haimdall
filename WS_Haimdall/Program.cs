using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using WS_Haimdall;

class Program
{
    public static void Main(string[] args)
    {
        // -------------------------------
        // Setup Serilog
        // -------------------------------
        string basePath = AppContext.BaseDirectory;
        string logFolder = Path.Combine(basePath, "logs");
        Directory.CreateDirectory(logFolder);

        string logFile = Path.Combine(logFolder, "app_log.txt");

        Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Fatal)
    .WriteTo.Console()
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Year)
    .CreateLogger();

        //Log.Logger = new LoggerConfiguration()
        //    .MinimumLevel.Information()
        //    .WriteTo.Console()
        //    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
        //    .CreateLogger();

        Log.Information("Service starting...");

        // -------------------------------
        // Create .NET 8 Host
        // -------------------------------
        var builder = Host.CreateApplicationBuilder(args);

        // Enable Windows Service
        builder.Services.AddWindowsService(options =>
        {
            options.ServiceName = "WS_Haimdall";
        });

        // Replace default logging with Serilog
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger);

        //// Disable default host startup messages
        //builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

        // Register worker
        builder.Services.AddHostedService<Worker>();

        // Build + Run
        var host = builder.Build();

        try
        {
            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Service terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
