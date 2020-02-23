#!/bin/bash

response=$(curl -X GET "http://localhost:$1/category/all" -H 'Content-Type: application/json')
echo $response