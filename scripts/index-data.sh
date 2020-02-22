#!/bin/bash

curl -X PUT "http://localhost:32795/sysevents/doc/1" -H 'Content-Type: application/json' -d'
{
  "category": "Service Deployment",
  "target_key": "my-service",
  "level": "information",
  "message": "my-service deployed with artifact artifacts/my-service:v_123",
  "sender": "rchall",
  "tags": [
    "deployment", "bug-fix", "artifacts/my-service:v_123" 
  ],
  "timestamp": "2020-02-22T08:13:28Z"
}'