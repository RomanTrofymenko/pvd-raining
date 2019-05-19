using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public class ProvidenceRainCheckWorker : IProvidenceRainCheckWorker
    {
        private readonly string[] rainingMessages = new string[] { "Yes.", "Definitely yes.", "Of course.", "Of course it is.", "You bet.", "Of course it is, what did you think?", "Yep.", "Yes. Yes, it is raining in Providence. Huge surprise." };
        private readonly string[] notRainingMessages = new string[] { "Not yet.", "No, but it will soon.", "As surprising as it is - no, it is not.", "No... Weird.", "You probably won't believe me, but it actually isn't.", "No. I know, right?", "It's not. Something's clearly wrong here." };
        private readonly IWeatherChecker weatherChecker;
        private readonly ITwitterApi twitterApi;
        private readonly ILogger logger;

        public ProvidenceRainCheckWorker(IWeatherChecker weatherChecker, ITwitterApi twitterApi, ILogger<ProvidenceRainCheckWorker> logger)
        {
            this.logger = logger;
            this.weatherChecker = weatherChecker;
            this.twitterApi = twitterApi;
        }

        public async Task Execute()
        {
            while (true)
            {
                logger.Log(LogLevel.Information, "Fetching weather data");
                var isRaining = await weatherChecker.RainCheck();
                if (isRaining.HasValue)
                {
                    logger.Log(LogLevel.Information, "Weather data fetched");
                    
                    logger.Log(LogLevel.Information, "Posting status update");
                    var success = await PostStatusUpdate(isRaining.Value, Enumerable.Empty<string>());
                    if (success)
                        logger.Log(LogLevel.Information, "Status update posted");
                    else
                        logger.Log(LogLevel.Warning, "Ran out of available messages");
                }
                Thread.Sleep(TimeSpan.FromHours(6));
            }
        }

        private async Task<bool> PostStatusUpdate(bool isRaining, IEnumerable<string> rejected)
        {
            var message = GetRandomMessage(isRaining, rejected);

            if (message == null)
                return false;

            var result = await twitterApi.Tweet(message);

            logger.Log(LogLevel.Information, $"Attempt to post message \"{message}\" recieved following response :\n {result}");

            var errors = TweetError.FromJson(result);

            if (errors.Errors != null && errors.Errors.Any() && errors.Errors[0].Code == 187)
                return await PostStatusUpdate(isRaining, rejected.Append(message));

            return true;
        }

        private string GetRandomMessage(bool isRaining, IEnumerable<string> rejected)
        {
            string[] options;

            if (isRaining)
                options = rainingMessages;
            else
                options = notRainingMessages;

            if (options.All(o => rejected.Contains(o)))
                return null;

            string candidate;
            do
                candidate = GetRandomString(options);
            while (rejected.Contains(candidate));

            return candidate;
        }

        private string GetRandomString(string[] options) => options[new Random().Next(options.Length)];
    }
}
