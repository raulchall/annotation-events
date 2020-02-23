using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SystemEvents.Models;
using SystemEvents.Models.Slack;
using SystemEvents.Services;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class CategorySubscriptionNotifier : ICategorySubscriptionNotifier
    {
        private readonly ILogger<CategorySubscriptionNotifier> _logger;
        private readonly IAdvanceConfiguration _advanceConfiguration;
        private readonly SlackService _slackService;
        private readonly IMonitoredAmazonSimpleNotificationService _amazonSimpleNotificationService;

        private IDictionary<string, List<CategorySubscription>> _configurationToNotificationChannelMap;

        private const string _onEventCreateNotificationMessage = "New System Event Created";
        private const string _onEventStartedNotificationMessage = "New System Event Started";
        private const string _onEventFinishedNotificationMessage = "System Event Finished";

        private const string _SystemName = "SystemEvents";
        private const string _SystemIcon = ":loudspeaker:";

        public CategorySubscriptionNotifier(
            ILogger<CategorySubscriptionNotifier> logger,
            IAdvanceConfiguration advanceConfiguration,
            SlackService slackService,
            IMonitoredAmazonSimpleNotificationService amazonSimpleNotificationService)
        {
            _logger                          = logger ?? 
                                                    throw new ArgumentNullException(nameof(logger));
            _advanceConfiguration            = advanceConfiguration ?? 
                                                    throw new ArgumentNullException(nameof(advanceConfiguration));
            _slackService                    = slackService ?? 
                                                    throw new ArgumentNullException(nameof(slackService));
            _amazonSimpleNotificationService = amazonSimpleNotificationService ?? 
                                                    throw new ArgumentNullException(nameof(amazonSimpleNotificationService));

            LoadMap();
        }

        public Task OnEventCreated(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            return SendNotification(
                    _onEventCreateNotificationMessage,
                    category, document, cancellationToken);
        }

        public Task OnEventStarted(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            return SendNotification(
                    _onEventStartedNotificationMessage,
                    category, document, cancellationToken);
        }

        public Task OnEventFinished(string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            return SendNotification(
                    _onEventFinishedNotificationMessage,
                    category, document, cancellationToken);
        }

        private async Task SendNotification(string notificationMessage, string category, SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return;
            }

            if (!_configurationToNotificationChannelMap.ContainsKey(category))
            {
                return;
            }

            var categorySubscriptions = _configurationToNotificationChannelMap[category];

            foreach (var categorySubscription in categorySubscriptions)
            {
                switch(categorySubscription.Type)
                {
                    case NotificationChannelType.Slack:
                        await SendSlackNotification(notificationMessage, categorySubscription, document, cancellationToken);
                        break;
                    case NotificationChannelType.Sns:
                        await SendSnsNotification(notificationMessage, categorySubscription, document, cancellationToken);
                        break;
                    default:
                        _logger.LogInformation(
                            $"The provided notification channel {categorySubscription.Type} is not supported");
                        break;
                }
            }
        }

        private Task SendSnsNotification(string notificationMessage, CategorySubscription categorySubscription, 
                SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            var message = JsonConvert.SerializeObject(new {
                Event = document,
                notificationMessage
            });

            return _amazonSimpleNotificationService.PublishAsync(
                    categorySubscription.TopicArn, message, cancellationToken);
        }

        private Task<string> SendSlackNotification(string notificationMessage, CategorySubscription categorySubscription, 
                SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            var message = new Message(notificationMessage)
                .SetUserWithEmoji(_SystemName, _SystemIcon);
            
            message.AddAttachment(new Attachment()
                .AddField("Event", document.Message, true)
                .AddField("Target", document.TargetKey, true)
                .AddField("Sender", document.Sender, true)
                .AddField("Start Time", document.Timestamp, true)
                .AddField("End Time", document.Endtime, true)
                .AddField("Level", document.Level, true)
                .SetColor( (document.Level == "critical")? "#eb4034": "#349ceb")
            );

            return _slackService.SendAsync(message, categorySubscription.WebhookUrl);
        }

        private void LoadMap()
        {
            if (_advanceConfiguration.Subscriptions == null)
            {
                return;
            }

            _configurationToNotificationChannelMap = new Dictionary<string, List<CategorySubscription>>();

            foreach (var categorySubscription in _advanceConfiguration.Subscriptions)
            {
                if (!_configurationToNotificationChannelMap.ContainsKey(categorySubscription.Category))
                {
                    _configurationToNotificationChannelMap[categorySubscription.Category] = new List<CategorySubscription>();
                }

                _configurationToNotificationChannelMap[categorySubscription.Category].Add(categorySubscription);
            }
        }
    }
}