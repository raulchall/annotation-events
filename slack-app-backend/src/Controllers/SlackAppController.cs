using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlackAppBackend.Configuration;
using SlackAppBackend.Enums;
using SlackAppBackend.Models.Slack;
using SlackAppBackend.Services;
using SlackAppBackend.Utils.Interfaces;
using SystemEvents.Api.Client.CSharp.Contracts;

namespace SlackAppBackend.Controllers
{
    [ApiController]
    public class SlackAppController : ControllerBase
    {
        private readonly ILogger<SlackAppController> _logger;
        private readonly SlackApiService _slackApiService;
        private readonly IMonitoredSystemEventServiceClient _systemEventClient;
        private readonly ISlackModalTemplateBuilder _slackModalTemplateBuilder;
        private readonly IAppConfiguration _configuration;

        public SlackAppController(
            ILogger<SlackAppController> logger,
            SlackApiService slackApiService,
            IMonitoredSystemEventServiceClient systemEventClient,
            ISlackModalTemplateBuilder slackModalTemplateBuilder,
            IAppConfiguration configuration)
        {
            _logger                    = logger 
                                            ?? throw new ArgumentNullException(nameof(logger));
            _slackApiService           = slackApiService 
                                            ?? throw new ArgumentNullException(nameof(slackApiService));
            _systemEventClient         = systemEventClient 
                                            ?? throw new ArgumentNullException(nameof(systemEventClient));
            _slackModalTemplateBuilder = slackModalTemplateBuilder 
                                            ?? throw new ArgumentNullException(nameof(slackModalTemplateBuilder));
            _configuration             = configuration 
                                            ?? throw new ArgumentNullException(nameof(configuration));
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
                return BadRequest($"`{nameof(request)}` can not be null");
            }

            if (string.IsNullOrWhiteSpace(request.text))
            {
                return BadRequest($"{nameof(request.text)} can not be null or whitespace");
            }

            var command = GetSlackCommand(request.text);
            if (command == null)
            {
                throw new ArgumentException($"Unrecognized command {request.text}");
            }

            switch(command.Item1)
            {
                case SlackCommand.Create:
                    var categories = await _systemEventClient.CategoryAllGetAsync();

                    var view = _slackModalTemplateBuilder.GetDialogTemplateWithCategories(
                        categories.Where(c => c.Name != "*" && c.SlackApp).ToList());

                    var response = await _slackApiService.OpenDialogAsync(new OpenSlackModalRequest
                    {
                        trigger_id = request.trigger_id,
                        view = view
                    });
                    break;
                case SlackCommand.End:
                    await _systemEventClient.EventEndPostAsync(command.Item2);
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
            try
            {
                if (_configuration.ShowPredefinedCategory)
                {
                    category = values.event_category_select.category.selected_option.value.Trim();
                }
            }
            catch
            {
                if (!_configuration.ShowCustomCategory)
                {
                    return Ok(new {
                        response_action = "errors",
                        errors = new {
                            event_category_select = "Select a category for your event",
                        }
                    });
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(category) && _configuration.ShowCustomCategory)
                {
                    category = values.event_custom_category.custom_category.value.Trim();
                }
            }
            catch
            {
                return Ok(new {
                    response_action = "errors",
                    errors = new {
                        event_custom_category = "Enter a category for your event",
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                return Ok(new {
                    response_action = "errors",
                    errors = new {
                        event_category_select = "Select a category for your event",
                        event_custom_category = "Enter a category for your event",
                    }
                });
            }

            var tags = new List<string>();
            if (!string.IsNullOrWhiteSpace(values.event_tags.tags.value))
            {
                tags = values.event_tags.tags.value
                            .Split(",")
                            .Select(e => e.Trim())
                            .Where(e => !string.IsNullOrWhiteSpace(e))
                            .ToList();
            }

            if (string.IsNullOrWhiteSpace(values?.event_target?.target?.value))
            {
                return Ok(new {
                    response_action = "errors",
                    errors = new {
                        event_target = "Enter a target for your event"
                    }
                });
            }

            if (string.IsNullOrWhiteSpace(values?.event_message?.message?.value))
            {
                return Ok(new {
                    response_action = "errors",
                    errors = new {
                        event_message = "Enter a message for your event"
                    }
                });
            }

            var model = new EventRequestModel
            {
                Category = category,
                TargetKey = values.event_target.target.value,
                Level = GetLevel(values.event_level.level.selected_option.value),
                Message = values.event_message.message.value,
                Sender = payload.user.username,
                Tags = new ObservableCollection<string>(tags)
            };

            await _systemEventClient.EventPostAsync(model);
            return Ok(new {
                response_action = "clear"
            });
        }

        private SystemEvents.Api.Client.CSharp.Contracts.Level GetLevel(string level)
        {
            var parsed = Enum.TryParse(level, ignoreCase: true, out SystemEvents.Api.Client.CSharp.Contracts.Level result);
            if (!parsed)
            {
                return SystemEvents.Api.Client.CSharp.Contracts.Level.Information;
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