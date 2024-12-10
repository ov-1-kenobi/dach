param environment string
param location string
param functionAppServicePlanId string
param storageAccountConnectionString string

resource fileUploadFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: 'DACH-file-upload-${environment}'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: functionAppServicePlanId
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

output fileUploadUrl string = 'https://${fileUploadFunction.properties.defaultHostName}'
