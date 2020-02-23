
using System;

namespace SystemEvents.Configuration
{
    public class AppConfiguration : IAppConfiguration, IElasticsearchClientConfiguration
    {
        public string AdvanceConfigurationPath => Environment.GetEnvironmentVariable("AdvanceConfigurationPath");

        public string UrlCsv => Environment.GetEnvironmentVariable("ELASTICSEARCH_URL_CSV");

        public string DefaultIndex => Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX");

        public int ClientTimeoutInMilliseconds => int.Parse(Environment.GetEnvironmentVariable("ELASTICSEARCH_TIMEOUT_MS"));

        public string DatetimeFieldFormat => Environment.GetEnvironmentVariable("ELASTICSEARCH_DATETIME_FORMAT");
    }
}
