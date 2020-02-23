using System;
using System.IO;
using SystemEvents.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SystemEvents.Utils
{
    public class YmlConfigurationLoader
    {
        public IAdvanceConfiguration LoadFromPath(string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"File {path} does not exist");
            }

            var input = new StreamReader(path);
            return LoadFromStreamReader(input);
        }

        private IAdvanceConfiguration LoadFromStreamReader(StreamReader input)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties().Build();
            
            return deserializer.Deserialize<AdvancedConfiguration>(input);
        }
    }
}