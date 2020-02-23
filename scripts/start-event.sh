#!/bin/bash

eventId=$(curl -X POST "http://localhost:$1/event/start" -H 'Content-Type: application/json' -d'
{
  "category": "Network Maintenance",
  "targetKey": "ha-proxy",
  "level": "critical",
  "message": "HA-Proxy maintenance",
  "sender": "rchall",
  "tags": [
    "maintenance", "networking", "team-a"
  ]
}' | jq -r '.eventId')

echo "Event Id is $eventId. Execute 'sh end-event.sh $1 $eventId' to mark the event as finished"