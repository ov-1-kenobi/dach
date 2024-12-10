param environment string
param location string
param appServicePlanId string
param storageAccountConnectionString string

resource todoApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'DACH-todo-${environment}'
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
          name: 'StorageAccountConnectionString'
          value: storageAccountConnectionString
        }
      ]
    }
  }
}

output todoAppUrl string = 'https://${todoApp.properties.defaultHostName}'
