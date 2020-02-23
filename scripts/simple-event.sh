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
