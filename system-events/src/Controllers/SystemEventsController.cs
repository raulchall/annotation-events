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

namespace SystemEvents.Controllers
{
    [ApiController]
    public class SystemEventsController : ControllerBase
    {
        private readonly ILogger<SystemEventsController> _logger;
        private readonly IElasticClient _esClient;
        private readonly IElasticsearchTimeStampFactory _timeStampFactory;

        public SystemEventsController(
            ILogger<SystemEventsController> logger,
            IElasticClient esClient,
            IElasticsearchTimeStampFactory timeStampFactory)
        {
            _logger           = logger ?? throw new ArgumentNullException(nameof(logger));
            _esClient         = esClient ?? throw new ArgumentNullException(nameof(esClient));
            _timeStampFactory = timeStampFactory ?? throw new ArgumentNullException(nameof(timeStampFactory));
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
            
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            var systemEvent = new
            {
                category = model.Category,
                level = model.Level.ToString(),
                target_key = model.TargetKey,
                message = $"{model.Message} by {model.Sender}",
                tags = model.Tags,
                sender = model.Sender,
                remoteIpAddress = remoteIpAddress.ToString(),
                timestamp = _timeStampFactory.GetTimestamp()
            };

            var response = await _esClient.IndexAsync(systemEvent, cancellationToken: cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogDebug(response.DebugInformation);
                _logger.LogError(response.OriginalException, "System event was not created");
                return BadRequest("System event was not created");
            }

            return Ok();
        }

        private bool IsValid(EventRequestModel model, out string reason)
        {
            
            if (string.IsNullOrWhiteSpace(model.Category))
            {
                reason = $"{nameof(model.Category)} can not be null or whitespace";
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
            
            reason = null;
            return true;
        }
    }
}
