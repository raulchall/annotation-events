
using System;

namespace SlackAppBackend.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        public Uri SystemEventServiceUri => new Uri(Environment.GetEnvironmentVariable("SYSTEM_EVENTS_SVC_URI"));
        public string SlackBotUserOAuthAccessToken => Environment.GetEnvironmentVariable("SLACK_OAUTHACCESS_TOKEN");
    }
}
