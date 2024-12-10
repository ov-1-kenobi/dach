#!/bin/bash

if [ -z "$1" ]; then
  echo "Usage: ./teardown.sh <environment>"
  exit 1
fi

environment=$1
resourceGroup="DACH-$environment-rg"

az group delete --name $resourceGroup --yes --no-wait
