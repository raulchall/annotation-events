using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SystemEvents.Models.Slack;
using SystemEvents.Utils;

namespace SystemEvents.Services
{
    public class SlackWebhookService : MonitoredClientBase
    {
        private readonly HttpClient _client;

        private const string _clientName = "SlackWebhookClient";
        private const string _sendMessageMethodName = "SendMessageAsync";

        public SlackWebhookService(
            ILogger<SlackWebhookService> logger,
            HttpClient client) : base (logger)
        {
            client.BaseAddress = new Uri("https://hooks.slack.com/services/");
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            _client = client;
        }

        /// <summary>
        /// Sends a slack message
        /// </summary>
        /// <param name="message">The message you would like to send to slack</param>
        /// <returns>The response body from the server</returns>
        public async Task<string> SendAsync(Message message, string webHookUrl)
        {
            var requestBody = JsonConvert.SerializeObject(message);
            var context = new { requestBody };

            var latency = PreInvoke(_clientName, _sendMessageMethodName, context);
            try
            {
                var result = await ProcessRequestAsync(webHookUrl, requestBody);
                PostInvokeSuccess(latency, _clientName, _sendMessageMethodName, context);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _sendMessageMethodName, ex, context);
                throw;
            }
        }

        /// <summary>
        ///     The method used to send the message data to the slack web hook, using ConfigureAwait(false)
        /// </summary>
        /// <param name="webHookUrl">The web hook url to send the message to</param>
        /// <param name="requestBody">The message model in json format</param>
        /// <returns></returns>
        private async Task<string> ProcessRequestAsync(string webHookUrl, string requestBody)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(webHookUrl)))
                using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
                {
                    request.Content = content;
                    using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                return "Error when posting to the web hook url. Error Message: " + ex.Message;
            }
        }
    }
}