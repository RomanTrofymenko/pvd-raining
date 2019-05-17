using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public class ProvidenceRainCheckWorker : IProvidenceRainCheckWorker
    {
        private readonly IWeatherChecker weatherChecker;
        private readonly ITwitterApi twitterApi;

        public ProvidenceRainCheckWorker(IWeatherChecker weatherChecker, ITwitterApi twitterApi)
        {
            this.weatherChecker = weatherChecker;
            this.twitterApi = twitterApi;
        }

        public async Task Execute()
        {
            while (true)
            {

                var isRaining = await weatherChecker.RainCheck();
                if (isRaining.HasValue)
                {
                    var message = GetRandomMessage(isRaining.Value);
                    var result = await twitterApi.Tweet(message);
                }
                Thread.Sleep(TimeSpan.FromHours(6));
            }
        }

        private static string GetRandomMessage(bool isRaining)
        {
            string[] options;

            if (isRaining)
                options = new string[] { "Yes.", "Definitely yes.", "Of course.", "Of course it is.", "Of course it fucking is.", "You bet." };
            else
                options = new string[] { "No... Weird.", "Not yet.", "No, but it will soon." };

            return GetRandomString(options);
        }

        private static string GetRandomString(string[] options) => options[new Random().Next(options.Length - 1)];
    }
}
