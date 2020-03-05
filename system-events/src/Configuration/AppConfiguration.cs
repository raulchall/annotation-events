
using System;

namespace SystemEvents.Configuration
{
    public class AppConfiguration : IAppConfiguration, IElasticsearchClientConfiguration, ISlackApiConfiguration
    {
        public string AdvanceConfigurationPath => Environment.GetEnvironmentVariable("AdvanceConfigurationPath");

        public string UrlCsv => Environment.GetEnvironmentVariable("ELASTICSEARCH_URL_CSV");

        public string DefaultIndex => Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX");

        public int ClientTimeoutInMilliseconds => int.Parse(Environment.GetEnvironmentVariable("ELASTICSEARCH_TIMEOUT_MS"));

        public string DatetimeFieldFormat => Environment.GetEnvironmentVariable("ELASTICSEARCH_DATETIME_FORMAT");

        public string IndexPatternPrefix => Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX_PATTERN_PREFIX");

        public string IndexPatternSuffixFormat => Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX_PATTERN_SUFFIX_FORMAT");

        public string SlackBotUserOAuthAccessToken => Environment.GetEnvironmentVariable("SLACK_OAUTHACCESS_TOKEN");
    }
}
