#!/bin/bash

response=$(curl -X GET "http://localhost:$1/category/subscriptions" -H 'Content-Type: application/json')
echo $response