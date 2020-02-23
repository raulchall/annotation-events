using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class MonitoredAmazonSimpleNotificationService : MonitoredClientBase, IMonitoredAmazonSimpleNotificationService
    {
        private readonly IAmazonSimpleNotificationService _client;

        private const string _clientName = "AmazonSimpleNotificationService";
        private const string _publishMethodName = "PublishAsync";
        
        public MonitoredAmazonSimpleNotificationService(
            ILogger<MonitoredAmazonSimpleNotificationService> logger,
            IAmazonSimpleNotificationService client) : base (logger)
        {
            _client             = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<PublishResponse> PublishAsync(string topicArn, string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topicArn))
            {
                throw new ArgumentException(
                    $"{nameof(topicArn)} can not be null or whitespace");
            }

            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topicArn
            };

            var context = new { topicArn, message };

            var latency = PreInvoke(_clientName, _publishMethodName, context);
            try
            {
                var result = await _client.PublishAsync(request, cancellationToken);
                PostInvokeSuccess(latency, _clientName, _publishMethodName, context);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _publishMethodName, ex, context);
                throw;
            }
        }
    }
}