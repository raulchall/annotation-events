using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Prometheus;
using SystemEvents.Models;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class PrometheusMonitoredElasticsearchClient : IMonitoredElasticsearchClient
    {
        private readonly ILogger<PrometheusMonitoredElasticsearchClient> _logger;
        private readonly IElasticClient _esClient;

        private const string _clientName = "ElasticsearchClient";
        private const string _indexMethodName = "IndexAsync";
        private const string _updateMethodName = "UpdateAsync";

        private readonly Counter _attemptCounter;
        private readonly Counter _successCounter;
        private readonly Counter _failCounter;
        private readonly Histogram _durationHistogram;

        public PrometheusMonitoredElasticsearchClient(
            ILogger<PrometheusMonitoredElasticsearchClient> logger,
            IElasticClient esClient)
        {
            _logger                = logger ?? throw new ArgumentNullException(nameof(logger));
            _esClient              = esClient ?? throw new ArgumentNullException(nameof(esClient));

            _attemptCounter = Prometheus.Metrics.CreateCounter(
                    $"client_attempt_total",
                    $"Total number of attempt to execute a client operation",
                    "Client", "Method");
            
            _successCounter = Prometheus.Metrics.CreateCounter(
                    $"client_success_total",
                    $"Total number of successes to execute a client operation",
                    "Client", "Method");
            
            _failCounter = Prometheus.Metrics.CreateCounter(
                    $"client_fail_total",
                    $"Total number of failures to execute a client operation",
                    "Client", "Method");
            
            _durationHistogram = Prometheus.Metrics.CreateHistogram(
                    $"client_duration_seconds",
                    $"Duration of client operation",
                    "Client", "Method");
        }

        public Task<IIndexResponse> IndexAsync(SystemEventElasticsearchDocument document, CancellationToken cancellationToken)
        {
            var context = new { document };

            var latency = PreInvoke(_clientName, _indexMethodName, context);
            try
            {
                var result = _esClient.IndexAsync(document, cancellationToken: cancellationToken);
                PostInvokeSuccess(latency, _clientName, _indexMethodName, context);
                return result;
            }
            catch (Exception ex)
            {
                PostInvokeFailure(_clientName, _indexMethodName, ex, context);
                throw;
            }
        }

        public Task<IUpdateResponse<SystemEventElasticsearchDocument>> UpdateAsync(string documentId, string indexName, SystemEventElasticsearchPartialDocument partialDocument, CancellationToken cancellationToken, int retryOnConflict = 3)
        {
            var context = new { documentId, indexName, partialDocument };

            var latency = PreInvoke(_clientName, _updateMethodName, context);
            try
            {
                var result = _esClient.UpdateAsync<SystemEventElasticsearchDocument, SystemEventElasticsearchPartialDocument>(
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

        public Stopwatch PreInvoke(string client, string method, object context = null)
        {
            _logger.LogTrace($"\"{client}.{method}\" called", context);
            _attemptCounter.WithLabels(client, method).Inc();
            return Stopwatch.StartNew();
        }

        public void PostInvokeSuccess(Stopwatch watch, string client, string method, object context = null)
        {
            _successCounter.WithLabels(client, method).Inc();
            _durationHistogram.WithLabels(client, method).Observe(watch.Elapsed.TotalSeconds);
            _logger.LogTrace($"\"{client}.{method}\" completed in {watch.ElapsedMilliseconds} ms", context);
        }

        public void PostInvokeFailure(string client, string method, Exception ex, object context = null)
        {
            _failCounter.WithLabels(client, method).Inc();
            _logger.LogError($"\"{client}.{method}\" failed", ex, context);
        }
    }
}