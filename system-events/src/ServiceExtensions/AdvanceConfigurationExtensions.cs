using Microsoft.Extensions.DependencyInjection;
using SystemEvents.Configuration;
using SystemEvents.Models;
using SystemEvents.Utils;

namespace SystemEvents.ServiceExtensions
{
    public static class AdvanceConfigurationExtensions
    {
        public static void AddAdvanceConfiguration(this IServiceCollection services, IAppConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.AdvanceConfigurationPath))
            {
                services.AddSingleton<IAdvanceConfiguration, AdvancedConfiguration>();
                return;
            }
            
            var loader = new YmlConfigurationLoader();
            var advancedConfiguration = loader.LoadFromPath(configuration.AdvanceConfigurationPath);
            services.AddSingleton<IAdvanceConfiguration>(advancedConfiguration);
        }
    }
}