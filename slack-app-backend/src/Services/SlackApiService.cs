using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlackAppBackend.Configuration;
using SlackAppBackend.Models.Slack;
using SlackAppBackend.Utils;

namespace SlackAppBackend.Services
{
    public class SlackApiService: MonitoredClientBase
    {
        private readonly IAppConfiguration _configuration;
        private readonly HttpClient _client;

        private const string _clientName = "SlackApiClient";

        private Uri _viewOpenUri = new Uri("https://slack.com/api/views.open");
        private const string _sendMessageMethodName = "SendMessageAsync";

        public SlackApiService(
            ILogger<SlackApiService> logger,
            IAppConfiguration configuration,
            HttpClient client) : base (logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            client.BaseAddress = new Uri("https://slack.com/api");
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            _client = client;
        }

        /// <summary>
        /// Sends a slack message
        /// </summary>
        /// <param name="message">The message you would like to send to slack</param>
        /// <returns>The response body from the server</returns>
        public async Task<string> OpenDialogAsync(OpenSlackModalRequest model)
        {
            var requestBody = JsonConvert.SerializeObject(model);

            var context = new { model.trigger_id };

            var latency = PreInvoke(_clientName, _sendMessageMethodName, context);
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, _viewOpenUri))
                {
                    request.Headers.Add("Authorization", $"Bearer {_configuration.SlackBotUserOAuthAccessToken}");
                    
                    using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
                    {
                        request.Content = content;
                        var response = await _client.SendAsync(request);
                        var result = await response.Content.ReadAsStringAsync();
                        PostInvokeSuccess(latency, _clientName, _sendMessageMethodName, context);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _sendMessageMethodName, ex, context);
                throw;
            }
        }

    }
}