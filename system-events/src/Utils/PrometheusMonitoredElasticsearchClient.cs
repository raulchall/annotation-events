using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SystemEvents.Models;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class PrometheusMonitoredElasticsearchClient : MonitoredClientBase, IMonitoredElasticsearchClient
    {
        private readonly IElasticClient _esClient;

        private const string _clientName = "ElasticsearchClient";
        private const string _indexMethodName = "IndexAsync";
        private const string _updateMethodName = "UpdateAsync";
        private const string _getMethodName = "GetAsync";

        public PrometheusMonitoredElasticsearchClient(
            ILogger<PrometheusMonitoredElasticsearchClient> logger,
            IElasticClient esClient) : base (logger)
        {
            _esClient              = esClient ?? throw new ArgumentNullException(nameof(esClient));
        }

        public async Task<SystemEventElasticsearchDocument> GetAsync(string documentId, CancellationToken cancellationToken)
        {
            var context = new { documentId };

            var latency = PreInvoke(_clientName, _getMethodName, context);
            try
            {
                var result = await _esClient.GetAsync<SystemEventElasticsearchDocument>(documentId,  
                                                                    cancellationToken: cancellationToken);
                PostInvokeSuccess(latency, _clientName, _getMethodName, context);
                return result.Source;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _getMethodName, ex, context);
                throw;
            }
        }

        public async Task<IIndexResponse> IndexAsync(SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            var context = new { document };

            var latency = PreInvoke(_clientName, _indexMethodName, context);
            try
            {
                var result = await _esClient.IndexAsync(document, cancellationToken: cancellationToken);
                PostInvokeSuccess(latency, _clientName, _indexMethodName, context);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _indexMethodName, ex, context);
                throw;
            }
        }

        public async Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateAsync(string documentId, string indexName, SystemEventElasticsearchPartialDocument partialDocument, CancellationToken cancellationToken, int retryOnConflict = 3)
        {
            var context = new { documentId, indexName, partialDocument };

            var latency = PreInvoke(_clientName, _updateMethodName, context);
            try
            {
                var result = await _esClient.UpdateAsync<SystemEventElasticsearchDocument, SystemEventElasticsearchPartialDocument>(
                        documentId, u => u
                        .Index(indexName)
                        .Doc(partialDocument)
                        .RetryOnConflict(retryOnConflict), cancellationToken: cancellationToken);
                PostInvokeSuccess(latency, _clientName, _updateMethodName, context);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _updateMethodName, ex, context);
                throw;
            }
        }
    }
}