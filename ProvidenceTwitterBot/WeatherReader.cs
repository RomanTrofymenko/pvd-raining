using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProvidenceTweeterBot
{
    public class WeatherChecker : IWeatherChecker
    {
        private readonly string apiKey;
        private readonly IHttpClientFactory httpClientFactory;

        public WeatherChecker(IOptions<ProvidenceRainCheckWorkerConfig> config, IHttpClientFactory httpClientFactory)
        {
            apiKey = config.Value.WeatherAppId;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<bool?> RainCheck()
        {
            bool? result = null;
            var baseUrl = "http://api.openweathermap.org/data/2.5/weather?id=5224162&APPID="+ apiKey;
            
            using (var client = httpClientFactory.CreateClient())
            using (var res = await client.GetAsync(baseUrl))
            using (var content = res.Content)
            {
                try
                {
                    var report = WeatherReport.FromJson(await content.ReadAsStringAsync());
                    result = report.Weather.Any(w => w.Main.Contains("Rain"));
                }
                catch
                { }
            }

            return result;
        }
    }
}