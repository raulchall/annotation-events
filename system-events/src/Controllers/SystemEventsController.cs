using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using SystemEvents.Models;
using SystemEvents.Utils.Interfaces;
using SystemEvents.Configuration;
using Nest;
using System.Collections.Generic;
using System.Linq;

namespace SystemEvents.Controllers
{
    [ApiController]
    public class SystemEventsController : ControllerBase
    {
        private readonly ILogger<SystemEventsController> _logger;
        private readonly IMonitoredElasticsearchClient _esClient;
        private readonly IElasticsearchTimeStampFactory _timeStampFactory;
        private readonly IElasticsearchClientConfiguration _esClientConfiguration;
        private readonly ICategorySubscriptionNotifier _categorySubscriptionNotifier;
        private readonly IAdvanceConfiguration _advanceConfiguration;

        public SystemEventsController(
            ILogger<SystemEventsController> logger,
            IMonitoredElasticsearchClient esClient,
            IElasticsearchTimeStampFactory timeStampFactory,
            IElasticsearchClientConfiguration esClientConfiguration,
            IAdvanceConfiguration advanceConfiguration,
            ICategorySubscriptionNotifier categorySubscriptionNotifier = null)
        {
            _logger                       = logger ?? throw new ArgumentNullException(nameof(logger));
            _esClient                     = esClient ?? throw new ArgumentNullException(nameof(esClient));
            _timeStampFactory             = timeStampFactory ?? throw new ArgumentNullException(nameof(timeStampFactory));
            _esClientConfiguration        = esClientConfiguration ?? throw new ArgumentNullException(nameof(esClientConfiguration));
            _advanceConfiguration         = advanceConfiguration ?? throw new ArgumentNullException(nameof(advanceConfiguration));
            _categorySubscriptionNotifier = categorySubscriptionNotifier;
        }

        /// <summary>
        /// Gets a system event by id
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult{SystemEventElasticsearchDocument}}"/>
        /// </returns>
        [HttpGet]
        [Route("event")]
        public async Task<ActionResult<SystemEventElasticsearchDocument>> GetEventById(string eventId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return BadRequest($"{nameof(eventId)} can not be null or whitespace");
            }

            var result = await _esClient.GetAsync(eventId, cancellationToken);
            if (result == null)
            {
                return BadRequest($"There is no event with id {eventId}");
            }

            return Ok(result);
        }

        /// <summary>
        /// Creates a new system event
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult{EventResponse}}"/>
        /// </returns>
        [HttpPost]
        [Route("event")]
        public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequestModel model, CancellationToken cancellationToken)
        {
            if (!Validate(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var response = await CreateSystemEvent(model, cancellationToken);
            if (!response.IsValid)
            {
                throw new ArgumentException("System event was not created");
            }

            if (_categorySubscriptionNotifier == null)
            {
                return Ok(new EventResponse{
                    EventId = response.Id
                });
            }

            // Notify about this event
            try
            {
                var document = await _esClient.GetAsync(response.Id, cancellationToken);
                await _categorySubscriptionNotifier.OnEventCreated(model.Category, document, cancellationToken);
            }
            catch (Exception exception)
            {
                // Ignore failure to send notification about system event
                _logger.LogError(exception, $"Error sending notifications for event with id {response.Id}");
            }

            return Ok(new EventResponse{
                EventId = response.Id
            });
        }

        /// <summary>
        /// Starts a new ongoing system event. In order to report the end of the 
        /// event keep the Event Id in the response.
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult{EventResponse}}"/>
        /// </returns>
        [HttpPost]
        [Route("event/start")]
        public async Task<ActionResult<EventResponse>> StartEvent([FromBody] EventRequestModel model, CancellationToken cancellationToken)
        {
            if (!Validate(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var response = await CreateSystemEvent(model, cancellationToken);
            if (!response.IsValid)
            {
                throw new ArgumentException("System event was not created");
            }

            if (_categorySubscriptionNotifier == null)
            {
                return Ok(new EventResponse{
                    EventId = response.Id
                });
            }

            // Notify about this event
            try
            {
                var document = await _esClient.GetAsync(response.Id, cancellationToken);
                await _categorySubscriptionNotifier.OnEventStarted(model.Category, document, cancellationToken);
            }
            catch (Exception exception)
            {
                // Ignore failure to send notification about system event
                _logger.LogError(exception, "Error sending notifications for event");
            }

            return Ok(new EventResponse{
                EventId = response.Id
            });
        }

        /// <summary>
        /// Registers the end of a given system event
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpPost]
        [Route("event/end")]
        public async Task<ActionResult> EndEvent(string eventId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return BadRequest($"{nameof(eventId)} can not be null or whitespace");
            }

            var response = await _esClient.UpdateAsync(
                        eventId,
                        new SystemEventElasticsearchPartialDocument { Endtime = _timeStampFactory.GetTimestamp()},
                        cancellationToken: cancellationToken, retryWithPreviousIndex: true);

            if (!response.IsValid)
            {
                throw new ArgumentException("System event was not updated");
            }

            if (_categorySubscriptionNotifier == null)
            {
                return Ok();
            }

            // Notify about this event
            try
            {
                var document = await _esClient.GetAsync(response.Id, cancellationToken);
                await _categorySubscriptionNotifier.OnEventFinished(document.Category, document, cancellationToken);
            }
            catch (Exception exception)
            {
                // Ignore failure to send notification about system event
                _logger.LogError(exception, "Error sending notifications for event");
            }

            return Ok();
        }

        private bool Validate(EventRequestModel model, out string reason)
        {
            if (model == null)
            {
                reason = $"{nameof(model)} can not be null";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(model.Category))
            {
                reason = $"{nameof(model.Category)} can not be null or whitespace";
                return false;
            }

            if (string.IsNullOrWhiteSpace(model.TargetKey))
            {
                reason = $"{nameof(model.TargetKey)} can not be null or whitespace";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(model.Message))
            {
                reason = $"{nameof(model.Message)} can not be null or whitespace";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(model.Sender))
            {
                reason = $"{nameof(model.Sender)} can not be null or whitespace";
                return false;
            }

            model.Category = model.Category.Trim();
            model.TargetKey = model.TargetKey.Trim();
            model.Sender = model.Sender.Trim();

            if (_advanceConfiguration?.Categories == null)
            {
                reason = null;
                return true;
            }

            var allAllowed = _advanceConfiguration.Categories.Any(c => c.Name == "*");

            // If Advance Configuration is enabled then only specified categories 
            // from the configuration can be used
            var category = _advanceConfiguration.Categories.FirstOrDefault(
                                                  c => c.Name == model.Category);
            if (!allAllowed && category == null)
            {
                reason = $"The provided category `{model.Category}` is not allowed. Check" + 
                        "/category/all for a list of allowed categories or contact your system administrator.";
                return false;
            }

            if (category?.Level != null && category.Level.Value != model.Level)
            {
                reason = $"Only events of level `{category.Level.Value}` are allowed for category `{model.Category}`. Check" + 
                        "/category/all for a list of allowed categories or contact your system administrator.";
                return false;
            }
            
            reason = null;
            return true;
        }

        private Task<IIndexResponse> CreateSystemEvent(EventRequestModel model, CancellationToken cancellationToken)
        {
            if (model.Tags == null)
            {
                model.Tags = new List<string>();
            }
            
            model.Tags.Add(model.Level.ToString());
            model.Tags.Add(model.Category);
            model.Tags.Add(model.TargetKey);

            // Remove duplicates
            var hashset = model.Tags.ToHashSet();

            var eventTimestamp = _timeStampFactory.GetTimestamp();
            var systemEvent = new SystemEventElasticsearchDocument
            {
                Category = model.Category,
                Level = model.Level.ToString(),
                TargetKey = model.TargetKey,
                Message = $"{model.Message} by {model.Sender}",
                Tags = hashset.ToArray(),
                Sender = model.Sender,
                Timestamp = eventTimestamp,
                Endtime = eventTimestamp
            };

            return _esClient.IndexAsync(systemEvent, cancellationToken: cancellationToken);
        }
    }
}
