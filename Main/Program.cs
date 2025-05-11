using Hangfire.Logging;
using Main;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /* THIS SERILOG WILL WORK WITH ONLY "Microsoft.Extensions.Logging" */
            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                  .MinimumLevel.Warning()
                  .WriteTo.File("Logs/Service.Clubeez.Log.txt", LogEventLevel.Warning,
                  flushToDiskInterval: TimeSpan.FromSeconds(1), rollingInterval: RollingInterval.Day)
                  //.WriteTo.Console()
                  .CreateLogger();

            /*Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                  .WriteTo.Console()
                  .CreateLogger();*/

            try
            {
                Log.Warning("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
