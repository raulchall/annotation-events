using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using SystemEvents.Configuration;

namespace SystemEvents.ServiceExtensions
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, 
                                    IElasticsearchClientConfiguration configuration)
        {
            var uris = GetUris(configuration.UrlCsv);
            
            if (string.IsNullOrWhiteSpace(configuration.DefaultIndex))
            {
                throw new ArgumentException(
                    "Missing or invalid value for environment variable ELASTICSEARCH_INDEX");
            }
            
            var connectionPool = new SniffingConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool).DefaultIndex(configuration.DefaultIndex);
            settings.RequestTimeout(TimeSpan.FromMilliseconds(configuration.ClientTimeoutInMilliseconds));
            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }

        private static IEnumerable<Uri> GetUris(string urlCsv)
        {
            if (string.IsNullOrWhiteSpace(urlCsv))
            {
                throw new ArgumentException(
                    "Missing or invalid value for environment variable ELASTICSEARCH_URL_CSV");
            }
            
            var splitted = urlCsv.Split(',');
            return splitted.Where(u => !string.IsNullOrWhiteSpace(u)).Select(
                u => new Uri(u)
            );
        }
    }
}