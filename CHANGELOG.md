Images released under this tags:

- Slack App Backend for System Events 
[raulchall/system-events-slack-backend:tagname](https://hub.docker.com/repository/docker/raulchall/system-events-slack-backend)

- System Events Service
[raulchall/system-events:tagname](https://hub.docker.com/repository/docker/raulchall/system-events)

# v5.8.0-alpha (03/08/2020)
## Added
- Slack App support 
- All category notification channel subscription
- Slack App Api Backend

# v5.7.1-alpha (02/26/2020)
## Fixed
- Bug adding duplicated level to event

# v5.7.0-alpha (02/26/2020)
## Added
- Support for indices with daily granularity. A prefix can be set using the `ELASTICSEARCH_INDEX_PATTERN_PREFIX`(Ex. `sysevents.`) environment variable and by using `ELASTICSEARCH_INDEX_PATTERN_SUFFIX_FORMAT` set to `YYYY.MM.DD` the service will construct the index name base on the current day Ex. `sysevents.2020.02.26`

# v5.6.8-alpha (02/25/2020)
## Fixed
- AWS Configuration is required with Advance configuration even if not used

# v5.6.7-alpha (02/24/2020)
## Added
- Health check endpoint /health
- `POST create` now returns the Event Id
- Advanced configuration now supports per category level restriction

# v5.6.6-alpha (02/17/2020)
## Added
- Supports sending event to Elastic Search 5.x.x
- Allows users subscribe using Slack or Amazon SNS to receive notifications of specific system events
- Allows admins to restrict the allowed categories