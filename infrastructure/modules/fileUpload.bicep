param environment string
param location string
param appServicePlanId string
param storageAccountConnectionString string

resource fileUploadApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'DACH-file-upload-${environment}'
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
          name: 'AzureWebJobsStorage'
          value: storageAccountConnectionString
        }
        {
          name: 'StorageAccountConnectionString'
          value: storageAccountConnectionString
        }
      ]
    }
  }
}

output fileUploadUrl string = 'https://${fileUploadApp.properties.defaultHostName}'
