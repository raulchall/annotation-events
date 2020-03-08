#!/bin/bash

export AdvanceConfigurationPath=./config.yml
export Serilog__MinimumLevel=Verbose
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_SUPPRESSSTATUSMESSAGES=true
export ELASTICSEARCH_URL_CSV=http://system-events-elasticsearch:9200/
export ELASTICSEARCH_INDEX=sysevents
export ELASTICSEARCH_TIMEOUT_MS=5000
export ELASTICSEARCH_DATETIME_FORMAT=yyyy-MM-dd'T'HH:mm:ssZ
export SLACK_OAUTHACCESS_TOKEN=xoxb

dotnet build system-events/clients/dotnet --configuration Release
