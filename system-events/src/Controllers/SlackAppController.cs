using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SystemEvents.Enums;
using SystemEvents.Models;
using SystemEvents.Models.Slack;
using SystemEvents.Services;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Controllers
{
    [ApiController]
    public class SlackAppController : ControllerBase
    {
        private readonly ILogger<SystemEventsController> _logger;
        private readonly SlackApiService _slackApiService;
        private readonly IAdvanceConfiguration _advanceConfiguration;
        private readonly ISystemEventSender _systemEventSender;

        public SlackAppController(
            ILogger<SystemEventsController> logger,
            SlackApiService slackApiService,
            IAdvanceConfiguration advanceConfiguration,
            ISystemEventSender systemEventSender)
        {
            _logger                 = logger ?? throw new ArgumentNullException(nameof(logger));
            _slackApiService        = slackApiService ?? throw new ArgumentNullException(nameof(slackApiService));
            _advanceConfiguration   = advanceConfiguration ?? throw new ArgumentNullException(nameof(advanceConfiguration));
            _systemEventSender  = systemEventSender ?? throw new ArgumentNullException(nameof(systemEventSender));
        }

        /// <summary>
        /// Get Modal template
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpGet]
        [Route("slack/template")]
        public ActionResult GetModalTemplate(CancellationToken cancellationToken)
        {
            var view = SlackModalTemplate.GetDialogTemplateWithCategories(
                _advanceConfiguration.Categories.Where(c => c.Name != "*").ToList());

            return Ok(view);
        }

        /// <summary>
        /// Creates or updates a system event
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpPost]
        [Route("slack/sysevent")]
        public async Task<ActionResult> CreateOrUpdateSystemEvent([FromForm] SystemEventRequestModel request, CancellationToken cancellationToken)
        {
            if(request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.text))
            {
                throw new ArgumentException($"{nameof(request.text)} can not be null or whitespace");
            }

            var command = GetSlackCommand(request.text);
            if (command == null)
            {
                throw new ArgumentException($"Unrecognized command {request.text}");
            }

            switch(command.Item1)
            {
                case SlackCommand.Create:
                    await _systemEventSender.Create(new EventRequestModel{

                    }, cancellationToken);

                    var view = SlackModalTemplate.GetDialogTemplateWithCategories(
                        _advanceConfiguration.Categories.Where(c => c.Name != "*").ToList());

                    var response = await _slackApiService.OpenDialogAsync(new OpenSlackModalRequest
                    {
                        trigger_id = request.trigger_id,
                        view = view
                    });
                    break;
                case SlackCommand.End:
                    await _systemEventSender.End(command.Item2, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"Unrecognized command {request.text}");
            }
            
            return Ok();
        }

        /// <summary>
        /// Creates or updates a system event
        /// </summary>
        /// <returns>
        ///   <seealso cref="Task{ActionResult}"/>
        /// </returns>
        [HttpPost]
        [Route("slack/sysevent/action")]
        public async Task<ActionResult> SlackInteractiveAction([FromForm] InteractivePayload request, CancellationToken cancellationToken)
        {
            var payload = JsonConvert.DeserializeObject<Payload>(request.payload);
            var values = payload.view.state.values;

            string category = null;
            if (!string.IsNullOrWhiteSpace(values.event_custom_category.custom_category.value))
            {
                category = values.event_custom_category.custom_category.value.Trim();
            }
            else
            {
                category = values.event_category_select.category.selected_option.value.Trim();
            }

            var tags = new List<string>();
            if (!string.IsNullOrWhiteSpace(values.event_tags.tags.value))
            {
                tags = values.event_tags.tags.value.Split(",").Select(e => e.Trim()).ToList();
            }

            var model = new EventRequestModel
            {
                Category = category,
                TargetKey = values.event_target.target.value,
                Level = GetLevel(values.event_level.level.selected_option.value),
                Message = values.event_message.message.value,
                Sender = payload.user.username,
                Tags = tags
            };

            var id = await _systemEventSender.Create(model, cancellationToken);
            return Ok(new {
                response_action = "clear"
            });
        }

        private Enums.Level GetLevel(string level)
        {
            var parsed = Enum.TryParse(level, ignoreCase: true, out Enums.Level result);
            if (!parsed)
            {
                return Enums.Level.Information;
            }

            return result;
        }

        private Tuple<SlackCommand, string> GetSlackCommand(string text)
        {
            var sb = new StringBuilder(text.Trim());
            
            if (text.StartsWith("new", ignoreCase: true, null))
            {
                sb.Remove(0, 3);
                return new Tuple<SlackCommand, string>(
                            SlackCommand.Create, sb.ToString().Trim());
            }

            if (text.StartsWith("end", ignoreCase: true, null))
            {
                sb.Remove(0, 3);
                return new Tuple<SlackCommand, string>(
                            SlackCommand.End, sb.ToString().Trim());
            }

            return null;
        }
    }
}