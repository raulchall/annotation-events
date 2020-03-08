
using System;

namespace SlackAppBackend.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public Uri SystemEventServiceUri => new Uri(Environment.GetEnvironmentVariable("SYSTEM_EVENTS_SVC_URI"));
        public string SlackBotUserOAuthAccessToken => Environment.GetEnvironmentVariable("SLACK_OAUTHACCESS_TOKEN");
        public string SlackSignedSecret => Environment.GetEnvironmentVariable("SLACK_SIGNED_SECRET");
        public bool ShowCustomCategory => AppConfigurationUtils.BooleanParseOrDefault("SLACK_TEMPLATE_OPTION_SHOW_CUSTOM_CATEGORY", true);
        public bool ShowPredefinedCategory => AppConfigurationUtils.BooleanParseOrDefault("SLACK_TEMPLATE_OPTION_SHOW_PREDEFINED_CATEGORY");
    }

    public static class AppConfigurationUtils
    {
        public static bool BooleanParseOrDefault(string environmentVariable, bool defaultValue = false)
        {
            var parsed = bool.TryParse(Environment.GetEnvironmentVariable(environmentVariable), out var result);
            if (!parsed)
            {
                return defaultValue;
            }

            return result;
        }
    }
}
