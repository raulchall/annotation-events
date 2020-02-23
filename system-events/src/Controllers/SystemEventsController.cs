using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using SystemEvents.Models;
using System.Net.Http;
using Nest;
using SystemEvents.Configuration;
using SystemEvents.Utils.Interfaces;
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
        /// Creates a new system event
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpPost]
        [Route("event")]
        public async Task<ActionResult> Create([FromBody] EventRequestModel model, CancellationToken cancellationToken)
        {
            if (!IsValid(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var response = await CreateSystemEvent(model, cancellationToken);
            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);
                _logger.LogError(response.OriginalException, "System event was not created");
                return BadRequest("System event was not created");
            }

            if (_categorySubscriptionNotifier == null)
            {
                return Ok();
            }

            // Notify about this event
            try
            {
                var document = await _esClient.GetAsync(response.Id, cancellationToken);
                await _categorySubscriptionNotifier.OnEventCreated(model.Category, document, cancellationToken);
            }
            catch
            {
                // Ignore failure to send notification about system event
            }

            return Ok();
        }

        /// <summary>
        /// Starts a new ongoing system event. In order to report the end of the 
        /// event keep the Event Id in the response.
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpPost]
        [Route("event/start")]
        public async Task<ActionResult<StartEventResponse>> StartEvent([FromBody] EventRequestModel model, CancellationToken cancellationToken)
        {
            if (!IsValid(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var response = await CreateSystemEvent(model, cancellationToken);
            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);
                _logger.LogError(response.OriginalException, "System event was not created");

                return BadRequest("Unable to start a system event");
            }

            if (_categorySubscriptionNotifier == null)
            {
                return Ok();
            }

            // Notify about this event
            try
            {
                var document = await _esClient.GetAsync(response.Id, cancellationToken);
                await _categorySubscriptionNotifier.OnEventStarted(model.Category, document, cancellationToken);
            }
            catch
            {
                // Ignore failure to send notification about system event
            }

            return Ok(new StartEventResponse{
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
                        _esClientConfiguration.DefaultIndex, 
                        new SystemEventElasticsearchPartialDocument { Endtime = _timeStampFactory.GetTimestamp()},
                        cancellationToken: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);
                _logger.LogError(response.OriginalException, "System event was not created");

                return BadRequest("Unable to mark system event as concluded");
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
            catch
            {
                // Ignore failure to send notification about system event
            }

            return Ok();
        }

        private async Task<IIndexResponse> CreateSystemEvent(EventRequestModel model, CancellationToken cancellationToken)
        {
            if (model.Tags == null)
            {
                model.Tags = new List<string>();
            }
            
            model.Tags.Add(model.Level.ToString());

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            var eventTimestamp = _timeStampFactory.GetTimestamp();

            var systemEvent = new SystemEventElasticsearchDocument
            {
                Category = model.Category,
                Level = model.Level.ToString(),
                TargetKey = model.TargetKey,
                Message = $"{model.Message} by {model.Sender}",
                Tags = model.Tags,
                Sender = model.Sender,
                RemoteIpAddress = remoteIpAddress.ToString(),
                Timestamp = eventTimestamp,
                Endtime = eventTimestamp
            };

            return await _esClient.IndexAsync(systemEvent, cancellationToken: cancellationToken);
        }

        private bool IsValid(EventRequestModel model, out string reason)
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

            // If Advance Configuration is enabled then only specified categories 
            // from the configuration can be used
            if (_advanceConfiguration.Categories!= null 
                    && !_advanceConfiguration.Categories.Any(c => c.Name == model.Category))
            {
                reason = $"The provided category `{model.Category}` is not allowed. Check" + 
                        "/category/all for a list of allowed categories or contact your system administrator.";

                return false;
            }
            
            reason = null;
            return true;
        }
    }
}
