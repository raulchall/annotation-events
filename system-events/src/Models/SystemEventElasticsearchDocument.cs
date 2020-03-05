using System.Collections.Generic;
using Newtonsoft.Json;

namespace SystemEvents.Models
{
    public class SystemEventElasticsearchDocument
    {
        [JsonProperty("_id")]
        public string Id { get;set; }

        [JsonProperty("category")]
        public string Category { get;set; }

        [JsonProperty("level")]
        public string Level { get;set; }

        [JsonProperty("target_key")]
        public string TargetKey { get;set; }

        [JsonProperty("message")]
        public string Message { get;set; }

        [JsonProperty("tags")]
        public string[] Tags { get;set; }

        [JsonProperty("sender")]
        public string Sender { get;set; }

        [JsonProperty("remoteIpAddress")]
        public string RemoteIpAddress { get;set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get;set; }

        [JsonProperty("endtime")]
        public string Endtime { get;set; }
    }

    public class SystemEventElasticsearchPartialDocument 
    {
        [JsonProperty("endtime")]
        public string Endtime { get;set; }
    }
}