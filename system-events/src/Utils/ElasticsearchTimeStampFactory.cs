using System;
using SystemEvents.Configuration;
using SystemEvents.Utils.Interfaces;

namespace SystemEvents.Utils
{
    public class ElasticsearchTimeStampFactory : IElasticsearchTimeStampFactory
    {
        private readonly IElasticsearchClientConfiguration _configuration;

        private DateTime _ephoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public ElasticsearchTimeStampFactory(
            IElasticsearchClientConfiguration configuration)
        {
            _configuration   = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GetTimestamp()
        {  
            try
            {
                if (_configuration.DatetimeFieldFormat == "epoch_second")
                {
                    var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    return dto.ToUnixTimeSeconds().ToString();
                }

                if (_configuration.DatetimeFieldFormat == "epoch_millis")
                {
                    var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    return dto.ToUnixTimeMilliseconds().ToString();
                }

                return DateTime.UtcNow.ToString(
                    _configuration.DatetimeFieldFormat);
            }
            catch (System.Exception)
            {
                var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
                return dto.ToUnixTimeSeconds().ToString();
            }
        }
    }
}