using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace ProvidenceTweeterBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";
            if (isDevelopment)
                configBuilder.AddUserSecrets<ProvidenceRainCheckWorkerConfig>();
            
            var configurationRoot = configBuilder.Build();
            
            var serviceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddScoped<IWeatherChecker, WeatherChecker>()
                .AddScoped<ITwitterApi, TwitterApi>()
                .AddScoped<IProvidenceRainCheckWorker, ProvidenceRainCheckWorker>()
                .Configure<ProvidenceRainCheckWorkerConfig>(configurationRoot.GetSection(nameof(ProvidenceRainCheckWorkerConfig)))
                .AddOptions()
                .BuildServiceProvider();

            var worker = serviceProvider.GetRequiredService<IProvidenceRainCheckWorker>();
            await worker.Execute();
        }
    }

    public class ProvidenceRainCheckWorkerConfig
    {
        public TwitterApiConfig TwitterApiConfig { get; set; }
        public string WeatherAppId { get; set; }
    }

    public class TwitterApiConfig
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}
