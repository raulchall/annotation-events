using System;

namespace SlackAppBackend.Configuration
{
    public interface IAppConfiguration
    {
        Uri SystemEventServiceUri { get; }

        string SlackBotUserOAuthAccessToken { get; }
    }
}