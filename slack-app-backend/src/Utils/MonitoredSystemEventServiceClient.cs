using SlackAppBackend.Utils.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SystemEvents.Api.Client.CSharp.Contracts;

namespace SlackAppBackend.Utils
{
    public partial class MonitoredSystemEventServiceClient : MonitoredClientBase, IMonitoredSystemEventServiceClient
    {
        private const string _clientName = "ElasticsearchClient";
        private const string _eventPostAsyncMethodName = "EventPostAsync";
        private const string _categoryAllGetAsyncMethodName = "CategoryAllGetAsync";
        private const string _eventEndPostAsyncMethodName = "EventEndPostAsync";

        private readonly ISystemEventsClient _systemEventsClient;
        private readonly ICategoriesClient _categoriesClient;

        public MonitoredSystemEventServiceClient(
            ILogger<MonitoredSystemEventServiceClient> logger,
            ISystemEventsClient systemEventsClient,
            ICategoriesClient categoriesClient
        ) : base(logger)
        {
            _systemEventsClient     = systemEventsClient ?? throw new ArgumentNullException(nameof(systemEventsClient));
            _categoriesClient       = categoriesClient ?? throw new ArgumentNullException(nameof(categoriesClient));
        }

        public async Task<EventResponse> EventPostAsync(EventRequestModel model)
        {
            var latency = PreInvoke(_clientName, _eventPostAsyncMethodName, model);
            try
            {
                var result = await _systemEventsClient.CreateAsync(model);
                PostInvokeSuccess(latency, _clientName, _eventPostAsyncMethodName, result);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _eventPostAsyncMethodName, ex, model);
                throw;
            }
        }

        public async Task EventEndPostAsync(string eventId)
        {
            var latency = PreInvoke(_clientName, _eventEndPostAsyncMethodName, eventId);
            try
            {
                await _systemEventsClient.EndEventAsync(eventId);
                PostInvokeSuccess(latency, _clientName, _eventEndPostAsyncMethodName, eventId);
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _eventEndPostAsyncMethodName, ex, eventId);
                throw;
            }
        }

        public async Task<ICollection<Category>> CategoryAllGetAsync()
        {
            var latency = PreInvoke(_clientName, _categoryAllGetAsyncMethodName);
            try
            {
                var result = await _categoriesClient.ListAsync();
                PostInvokeSuccess(latency, _clientName, _categoryAllGetAsyncMethodName);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _categoryAllGetAsyncMethodName, ex);
                throw;
            }
        }
    }
}