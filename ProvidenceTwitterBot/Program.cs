using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProvidenceTwitterBot.Config;

namespace ProvidenceTwitterBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            
            var configurationRoot = configBuilder.Build();

            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddScoped<IWeatherChecker, WeatherChecker>()
                .AddScoped<ITwitterApi, TwitterApi>()
                .AddScoped<IProvidenceRainCheckWorker, ProvidenceRainCheckWorker>()
                .AddLogging(loggingBuilder => loggingBuilder.AddConsole())
                .Configure<ProvidenceRainCheckWorkerConfig>(configurationRoot)
                .AddOptions()
                .BuildServiceProvider();

            var worker = serviceProvider.GetRequiredService<IProvidenceRainCheckWorker>();
            await worker.Execute();
        }
    }
}
