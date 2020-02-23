using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace SystemEvents.Utils
{
    public class MonitoredClientBase
    {
        protected readonly ILogger _logger;
        
        private readonly Counter _attemptCounter;
        private readonly Counter _successCounter;
        private readonly Counter _failCounter;
        private readonly Histogram _durationHistogram;


        public MonitoredClientBase(ILogger logger)
        {
            _logger                = logger ?? throw new ArgumentNullException(nameof(logger));

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

        protected Stopwatch PreInvoke(string client, string method, object context = null)
        {
            _logger.LogTrace($"\"{client}.{method}\" called", context);
            _attemptCounter.WithLabels(client, method).Inc();
            return Stopwatch.StartNew();
        }

        protected void PostInvokeSuccess(Stopwatch watch, string client, string method, object context = null)
        {
            _successCounter.WithLabels(client, method).Inc();
            _durationHistogram.WithLabels(client, method).Observe(watch.Elapsed.TotalSeconds);
            _logger.LogTrace($"\"{client}.{method}\" completed in {watch.ElapsedMilliseconds} ms", context);
        }

        protected void PostInvokeFailure(string client, string method, Exception ex, object context = null)
        {
            _failCounter.WithLabels(client, method).Inc();
            _logger.LogError($"\"{client}.{method}\" failed", ex, context);
        }
    }
}