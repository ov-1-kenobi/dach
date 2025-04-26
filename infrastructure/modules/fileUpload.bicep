param environment string
param location string
param appServicePlanId string
param storageAccountConnectionString string
param fileUploadB2C_Tenant string
param fileUploadB2C_Instance string
param fileUploadB2C_Domain string
param fileUploadB2C_ClientId string
@secure()
param fileUploadB2C_ClientSecret string
param fileUploadB2C_SignUpSignInPolicyId string
#disable-next-line secure-secrets-in-params //no secret
param fileUploadB2C_PasswordResetPolicyId string

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
        {
          name: 'AzureAdB2C__Tenant'
          value: fileUploadB2C_Tenant
        }
        {
          name: 'AzureAdB2C__Instance'
          value: fileUploadB2C_Instance
        }
        {
          name: 'AzureAdB2C__Domain'
          value: fileUploadB2C_Domain
        }
        {
          name: 'AzureAdB2C__ClientId'
          value: fileUploadB2C_ClientId
        }
        {
          name: 'AzureAdB2C__ClientSecret'
          value: fileUploadB2C_ClientSecret
        }
        {
          name: 'AzureAdB2C__SignUpSignInPolicyId'
          value: fileUploadB2C_SignUpSignInPolicyId
        }
        {
          name: 'AzureAdB2C__PasswordResetPolicyId'
          value: fileUploadB2C_PasswordResetPolicyId
        }
      ]
    }
  }
}

output fileUploadUrl string = 'https://${fileUploadApp.properties.defaultHostName}'
