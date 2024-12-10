param environment string
param location string
param appServicePlanId string
param signalRConnectionString string
param storageAccountConnectionString string

resource chatApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'DACH-chat-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
        {
          name: 'AzureSignalRConnectionString'
          value: signalRConnectionString
        }
        {
          name: 'StorageAccountConnectionString'
          value: storageAccountConnectionString
        }
      ]
    }
  }
}

output chatAppUrl string = 'https://${chatApp.properties.defaultHostName}'
