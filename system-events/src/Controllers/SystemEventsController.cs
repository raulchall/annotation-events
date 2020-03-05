using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using SystemEvents.Models;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Controllers
{
    [ApiController]
    public class SystemEventsController : ControllerBase
    {
        private readonly ILogger<SystemEventsController> _logger;
        private readonly ISystemEventSender _systemEventSender;

        public SystemEventsController(
            ILogger<SystemEventsController> logger,
            ISystemEventSender systemEventSender)
        {
            _logger                       = logger ?? throw new ArgumentNullException(nameof(logger));
            _systemEventSender            = systemEventSender ?? throw new ArgumentNullException(nameof(systemEventSender));
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

            var result = await _systemEventSender.Get(eventId, cancellationToken);
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
            if (!_systemEventSender.Validate(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var eventId = await _systemEventSender.Create(model, cancellationToken);
            return Ok(new EventResponse{
                EventId = eventId
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
            if (!_systemEventSender.Validate(model, out string reason))
            {
                return BadRequest($"Invalid request: {reason}");
            }

            var eventId = await _systemEventSender.Start(model, cancellationToken);
            return Ok(new EventResponse{
                EventId = eventId
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

            await _systemEventSender.End(eventId, cancellationToken);
            return Ok();
        }
    }
}
