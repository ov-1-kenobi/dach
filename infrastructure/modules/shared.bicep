param environment string
param location string

resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: 'asp-${environment}'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

// resource functionAppServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
//   name: 'func-asp-${environment}'
//   location: location
//   sku: {
//     name: 'Y1'
//     tier: 'Dynamic'
//   }
// }

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'storage${uniqueString(resourceGroup().id, environment)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-04-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            'https://dach-file-upload-uat.azurewebsites.net'
          ]
          allowedMethods: [
            'GET'
            'PUT'
            'POST'
            'OPTIONS'
            'HEAD'
          ]
          allowedHeaders: [
            '*'
          ]
          exposedHeaders: [
            '*'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

resource signalR 'Microsoft.SignalRService/SignalR@2022-02-01' = {
  name: 'signalr-${environment}'
  location: location
  sku: {
    name: 'Free_F1' // Free tier SKU for SignalR
    capacity: 1
  }
  properties: {
    features: [
      {
        flag: 'ServiceMode'
        value: 'Default'
      }
    ]
  }
}

var accountName = storageAccount.name
var accountKey = storageAccount.listKeys().keys[0].value
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${accountName};AccountKey=${accountKey};EndpointSuffix=core.windows.net'


output appServicePlanId string = appServicePlan.id
//output functionAppServicePlanId string = functionAppServicePlan.id
output storageAccountConnectionString string = storageConnectionString
//output storageAccountConnectionString string = listKeys(storageAccount.id, '2021-04-01').keys[0].value
output signalRConnectionString string = signalR.listKeys().primaryConnectionString
