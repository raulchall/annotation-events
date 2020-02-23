#!/bin/sh

aws --endpoint-url $AWS_SNS_ENDPOINT_URL sns create-topic --name system-event-network-maintenance
aws --endpoint-url $AWS_SNS_ENDPOINT_URL sns create-topic --name system-event-database-migration
