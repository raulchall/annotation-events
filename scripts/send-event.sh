#!/bin/bash

curl -X POST "http://localhost:$1/event" -H 'Content-Type: application/json' -d'
{
  "category": "Service Deployment",
  "targetKey": "my-service",
  "level": "information",
  "message": "Service my-service deployed with artifact my-service:v123",
  "sender": "rchall",
  "tags": [
    "deployment", "cool-team"
  ]
}'

eventId=$(curl -X POST "http://localhost:$1/event/start" -H 'Content-Type: application/json' -d'
{
  "category": "Network Maintenance",
  "targetKey": "ha-proxy",
  "level": "information",
  "message": "Started HA-Proxy maintenance",
  "sender": "rchall",
  "tags": [
    "maintenance", "networking", "team-a"
  ]
}' | jq -r '.eventId')

echo "Event Id is $eventId"

echo "Waiting for 10 seconds to send an end of event message"
sleep 10s

curl -X POST "http://localhost:$1/event/end?eventId=$eventId" -H 'Content-Type: application/json'