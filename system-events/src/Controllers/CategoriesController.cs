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
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly IAdvanceConfiguration _advanceConfiguration;

        public CategoriesController(
            ILogger<CategoriesController> logger,
            IAdvanceConfiguration advanceConfiguration,
            IElasticsearchTimeStampFactory timeStampFactory,
            IElasticsearchClientConfiguration esClientConfiguration)
        {
            _logger                = logger ?? throw new ArgumentNullException(nameof(logger));
            _advanceConfiguration = advanceConfiguration ?? throw new ArgumentNullException(nameof(advanceConfiguration));
        }

        /// <summary>
        /// Lists all the configured system event categories
        /// </summary>
        /// <returns>
        ///   <seealso cref="ActionResult{List{Category}}"/>
        /// </returns>
        [HttpGet]
        [Route("category/all")]
        public ActionResult<List<Category>> List()
        {
            if (_advanceConfiguration.Categories == null)
            {
                return Ok(new List<Category>());
            }

            return Ok(_advanceConfiguration.Categories);
        }

        /// <summary>
        /// Lists all subscriptions
        /// </summary>
        /// <returns>
        ///   <seealso cref="ActionResult{List{Category}}"/>
        /// </returns>
        [HttpGet]
        [Route("category/subscriptions")]
        public ActionResult<List<CategorySubscription>> ListSubscriptions()
        {
            if (_advanceConfiguration.Categories == null)
            {
                return Ok(new List<CategorySubscription>());
            }

            var subscriptions = new List<CategorySubscription>(_advanceConfiguration.Subscriptions);
            foreach (var subscription in subscriptions)
            {
                if (subscription.WebhookUrl == null)
                    continue;

                subscription.WebhookUrl = "https://hooks.slack.com/services/##############";
            }

            return Ok(subscriptions);
        }
    }
}