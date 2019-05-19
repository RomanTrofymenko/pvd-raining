using Microsoft.Extensions.Options;
using ProvidenceTwitterBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProvidenceTwitterBot
{
    public class TwitterApi : ITwitterApi
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string consumerKey, accessToken;
        private readonly HMACSHA1 sigHasher;

        public TwitterApi(IOptions<ProvidenceRainCheckWorkerConfig> config, IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            consumerKey = config.Value.ConsumerKey;
            accessToken = config.Value.AccessToken;

            sigHasher = new HMACSHA1(new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", config.Value.ConsumerSecret, config.Value.AccessTokenSecret)));
        }

        public async Task<string> Tweet(string text)
        {
            var fullUrl = "https://api.twitter.com/1.1/statuses/update.json";
            var data = new Dictionary<string, string> {
                { "status", text },
                { "trim_user", "1" }
            };
            var oAuthHeader = GenerateOAuthHeader(fullUrl, data);
            
            return await SendRequest(fullUrl, oAuthHeader, new FormUrlEncodedContent(data));
        }


        private string GenerateOAuthHeader(string fullUrl, Dictionary<string, string> payload)
        {
            var oAuthData = new Dictionary<string, string>();

            // Timestamps are in seconds since 1/1/1970.
            var timestamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            // Add all the OAuth headers we'll need to use when constructing the hash.
            oAuthData.Add("oauth_consumer_key", consumerKey);
            oAuthData.Add("oauth_signature_method", "HMAC-SHA1");
            oAuthData.Add("oauth_timestamp", timestamp.ToString());
            oAuthData.Add("oauth_nonce", GetNonce());
            oAuthData.Add("oauth_token", accessToken);
            oAuthData.Add("oauth_version", "1.0");

            // Generate the OAuth signature and add it to our payload.
            oAuthData.Add("oauth_signature", GenerateSignature(fullUrl, payload.Concat(oAuthData).ToDictionary(s => s.Key, s => s.Value)));

            return "OAuth " + string.Join(
                ", ",
                oAuthData
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}=\"{Uri.EscapeDataString(kvp.Value)}\"")
                    .OrderBy(s => s)
            );
        }

        private static string GetNonce() => Guid.NewGuid().ToString().Replace("-", "");

        private string GenerateSignature(string url, Dictionary<string, string> data)
        {
            var sigString = string.Join(
                "&",
                data
                    .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")
                    .OrderBy(s => s)
            );

            var fullSigData = $"POST&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(sigString)}";

            return Convert.ToBase64String(sigHasher.ComputeHash(new ASCIIEncoding().GetBytes(fullSigData.ToString())));
        }

        private async Task<string> SendRequest(string fullUrl, string oAuthHeader, FormUrlEncodedContent formData)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", oAuthHeader);
                httpClient.DefaultRequestHeaders.CacheControl.NoCache = true;
                var httpResp = await httpClient.PostAsync(fullUrl, formData);
                var respBody = await httpResp.Content.ReadAsStringAsync();

                return respBody;
            }
        }
    }
}