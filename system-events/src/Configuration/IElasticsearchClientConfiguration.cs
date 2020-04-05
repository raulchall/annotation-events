namespace SystemEvents.Configuration
{
    public interface IElasticsearchClientConfiguration
    {
        public string UrlCsv { get; }

        public string DefaultIndex { get; }

        public int ClientTimeoutInMilliseconds { get; }

        public string DatetimeFieldFormat { get; }

        public string IndexPatternPrefix { get; }

        public string IndexPatternSuffixFormat { get; }
    }
}