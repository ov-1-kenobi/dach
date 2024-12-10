#!/bin/bash

if [ -z "$1" ]; then
  echo "Usage: ./deploy.sh <environment> [projectNames]"
  exit 1
fi

environment=$1
projectNames=${2:-'["chat", "file-upload", "todo"]'}
parametersFile="./infrastructure/parameters/$environment.parameters.json"

az deployment sub create \
  --name DACHDeployment-$environment \
  --location eastus \
  --template-file ./infrastructure/main.bicep \
  --parameters @"$parametersFile" \
  --parameters projectNames="$projectNames"
