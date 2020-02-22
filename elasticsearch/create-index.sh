#!/bin/bash

curl -X DELETE "http://system-events-elasticsearch:9200/sysevents" -H 'Content-Type: application/json' 

DATE_FORMAT="yyyy-MM-dd'T'HH:mm:ssZZ"

curl -X PUT "http://system-events-elasticsearch:9200/sysevents" -H 'Content-Type: application/json' -d'
{
  "mappings": 
  {
    "system_events": {
      "properties": {
        "timestamp": {
          "type": "date",
          "format": "yyyy-MM-dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||'"$DATE_FORMAT"'||epoch_millis||epoch_second"
        },
        "category": { 
          "type": "text",
          "fields": {
            "keyword": { 
              "type": "keyword"
            }
          }
        },
        "target_key": { 
          "type": "text",
          "fields": {
            "keyword": { 
              "type": "keyword"
            }
          }
        },
        "level": { 
          "type": "text",
          "fields": {
            "keyword": { 
              "type": "keyword"
            }
          }
        }
      }
    }
  }
}'

curl -X PUT "http://system-events-elasticsearch:9200/sysevents/_doc/1" -H 'Content-Type: application/json' -d'
{
  
}'