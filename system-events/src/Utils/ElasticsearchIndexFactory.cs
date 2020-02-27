using System;
using Microsoft.Extensions.Logging;
using SystemEvents.Configuration;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class ElasticsearchIndexFactory : IElasticsearchIndexFactory
    {
        private readonly ILogger _logger;
        private readonly IElasticsearchClientConfiguration _elasticSearchClientconfiguration;
        private const string _dailyChunkFormat = "yyyy.MM.dd";

        public ElasticsearchIndexFactory(
            ILogger<ElasticsearchIndexFactory> logger,
            IElasticsearchClientConfiguration elasticSearchClientconfiguration)
        {
            _logger                            = logger ?? throw new ArgumentNullException(nameof(logger));
            _elasticSearchClientconfiguration  = elasticSearchClientconfiguration 
                                                    ?? throw new ArgumentNullException(nameof(elasticSearchClientconfiguration));
        }

        public string GetPreviousIndexName()
        {
            return GetIndexNameInternal(DateTime.UtcNow.AddDays(-1));
        }

        public string GetIndexName()
        {
            return GetIndexNameInternal(DateTime.UtcNow);
        }

        private string GetIndexNameInternal(DateTime utcDate)
        {
            if(!string.IsNullOrWhiteSpace(_elasticSearchClientconfiguration.IndexPatternPrefix))
            {
                var indexChunkSuffix = GetIndexNameSuffix(
                    _elasticSearchClientconfiguration.IndexPatternSuffixFormat,
                    utcDate);

                if (string.IsNullOrWhiteSpace(indexChunkSuffix))
                {
                    throw new ArgumentException($"Found index pattern prefix with value {_elasticSearchClientconfiguration.IndexPatternPrefix}" +
                        $" but no suffix could be calculated from the provided chunk suffix format {_elasticSearchClientconfiguration.IndexPatternSuffixFormat}");
                }

                return $"{_elasticSearchClientconfiguration.IndexPatternPrefix}{indexChunkSuffix}";
            }

            if (string.IsNullOrWhiteSpace(_elasticSearchClientconfiguration.DefaultIndex))
            {
                throw new ArgumentException($"Default Index can not be null or whitespace");
            }

            return _elasticSearchClientconfiguration.DefaultIndex;
        }

        private string GetIndexNameSuffix(string sufixPatternFormat, DateTime utcDate)
        {
            if (string.IsNullOrWhiteSpace(sufixPatternFormat))
            {
                return null;
            }

            // Calculate index chunk based on current date with daily granularity
            if (sufixPatternFormat.Equals(_dailyChunkFormat, StringComparison.InvariantCultureIgnoreCase))
            {
                return utcDate.ToString(_dailyChunkFormat);
            }

            return null;
        }
    }
}