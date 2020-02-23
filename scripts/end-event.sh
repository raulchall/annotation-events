#!/bin/bash

curl -X POST "http://localhost:$1/event/end?eventId=$2" -H 'Content-Type: application/json' -d ''