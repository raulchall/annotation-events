using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SystemEvents.Configuration;
using SystemEvents.Models;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class PrometheusMonitoredElasticsearchClient : MonitoredClientBase, IMonitoredElasticsearchClient
    {
        private readonly IElasticClient _esClient;
        private readonly IElasticsearchClientConfiguration _elasticSearchClientconfiguration;
        private readonly IElasticsearchIndexFactory _indexFactory;

        private const string _clientName = "ElasticsearchClient";
        private const string _indexMethodName = "IndexAsync";
        private const string _updateMethodName = "UpdateAsync";
        private const string _getMethodName = "GetAsync";

        

        public PrometheusMonitoredElasticsearchClient(
            ILogger<PrometheusMonitoredElasticsearchClient> logger,
            IElasticsearchClientConfiguration elasticSearchClientconfiguration,
            IElasticClient esClient,
            IElasticsearchIndexFactory indexFactory) : base (logger)
        {
            _esClient                              = esClient ?? throw new ArgumentNullException(nameof(esClient));
            _elasticSearchClientconfiguration      = elasticSearchClientconfiguration 
                                                        ?? throw new ArgumentNullException(nameof(elasticSearchClientconfiguration));
            _indexFactory                          = indexFactory 
                                                        ?? throw new ArgumentNullException(nameof(indexFactory));
        }

        public async Task<SystemEventElasticsearchDocument> GetAsync(string documentId, CancellationToken cancellationToken)
        {
            string indexName = null;
            var latency = PreInvoke(_clientName, _getMethodName, documentId);
            try
            {
                indexName = _indexFactory.GetIndexName();
                var result = await _esClient.GetAsync<SystemEventElasticsearchDocument>(
                                            new GetRequest<SystemEventElasticsearchDocument>(indexName, documentId),
                                            cancellationToken: cancellationToken);

                PostInvokeSuccess(latency, _clientName, _getMethodName, documentId, indexName);
                return result.Source;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _getMethodName, ex, documentId, indexName);
                throw;
            }
        }

        public async Task<IIndexResponse> IndexAsync(SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            string indexName = null;
            var latency = PreInvoke(_clientName, _indexMethodName, document);
            try
            {
                indexName = _indexFactory.GetIndexName();
                var result = await _esClient.IndexAsync( new IndexRequest<SystemEventElasticsearchDocument>(
                                                            document, indexName), cancellationToken: cancellationToken);

                PostInvokeSuccess(latency, _clientName, _indexMethodName, document, indexName);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _indexMethodName, ex, document, indexName);
                throw;
            }
        }

        public async Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateAsync(
                string documentId, SystemEventElasticsearchPartialDocument partialDocument, 
                CancellationToken cancellationToken, int retryOnConflict = 3, 
                bool retryWithPreviousIndex = false)
        {
            string indexName = null;
            var latency = PreInvoke(_clientName, _updateMethodName, documentId, partialDocument);
            try
            {
                indexName = _indexFactory.GetIndexName();
                Task<IUpdateResponse<SystemEventElasticsearchDocument>> updateResult = null;
                if (retryWithPreviousIndex)
                {
                    updateResult = UpdateWithRetryOnPreviousIndexAsync(
                        documentId, partialDocument, cancellationToken, retryOnConflict);
                }
                else
                {
                    updateResult = _esClient.UpdateAsync<SystemEventElasticsearchDocument, SystemEventElasticsearchPartialDocument>(
                        documentId, u => u
                        .Index(indexName)
                        .Doc(partialDocument)
                        .RetryOnConflict(retryOnConflict), cancellationToken: cancellationToken);
                }

                var result = await updateResult;
                
                PostInvokeSuccess(latency, _clientName, _updateMethodName, documentId, partialDocument, indexName);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _updateMethodName, ex, documentId, partialDocument, indexName);
                throw;
            }
        }

        public async Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateWithRetryOnPreviousIndexAsync(
                string documentId, SystemEventElasticsearchPartialDocument partialDocument, 
                CancellationToken cancellationToken, int retryOnConflict)
        {
            try
            {
                var indexName = _indexFactory.GetIndexName();
                return await _esClient.UpdateAsync<SystemEventElasticsearchDocument, SystemEventElasticsearchPartialDocument>(
                        documentId, u => u
                        .Index(indexName)
                        .Doc(partialDocument)
                        .RetryOnConflict(retryOnConflict), cancellationToken: cancellationToken);
            }
            catch 
            { 
                // Ignore
            }

            var previousIndexName = _indexFactory.GetPreviousIndexName();
            return await _esClient.UpdateAsync<SystemEventElasticsearchDocument, SystemEventElasticsearchPartialDocument>(
                    documentId, u => u
                    .Index(previousIndexName)
                    .Doc(partialDocument)
                    .RetryOnConflict(retryOnConflict), cancellationToken: cancellationToken);
        }


    }
}