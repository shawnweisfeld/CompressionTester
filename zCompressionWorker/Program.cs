using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace zCompressionWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService(
                        hostContext.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

                    var runConfig = new RunConfig();
                    hostContext.Configuration.GetSection("RunConfig").Bind(runConfig);
                    runConfig.RunID = Guid.NewGuid().ToString();
                    services.AddSingleton(runConfig);

                    services.AddSingleton<FileService>();
                    services.AddSingleton<DotNetCompressionService>();

                    services.AddHostedService<Worker>();
                });
    }
}
