// infrastructure/main.bicep

targetScope = 'subscription'

param environment string
param location string = 'eastus'  // Default location
param fileUploadB2C_Tenant string
param fileUploadB2C_Instance string
param fileUploadB2C_Domain string
param fileUploadB2C_ClientId string
@secure()
param fileUploadB2C_ClientSecret string
param fileUploadB2C_SignUpSignInPolicyId string
#disable-next-line secure-secrets-in-params //no secret
param fileUploadB2C_PasswordResetPolicyId string
param projectNames array = []

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'DACH-${environment}-rg'
  location: location
}

module sharedModule './modules/shared.bicep' = {
  name: 'sharedResources'
  scope: resourceGroup
  params: {
    environment: environment
    location: location
  }
}

module chatModule './modules/chat.bicep' = if (contains(projectNames, 'chat')) {
  name: 'chatModule'
  scope: resourceGroup
  params: {
    environment: environment
    location: location
    appServicePlanId: sharedModule.outputs.appServicePlanId
    signalRConnectionString: sharedModule.outputs.signalRConnectionString
    storageAccountConnectionString: sharedModule.outputs.storageAccountConnectionString
  }
}

module fileUploadModule './modules/fileUpload.bicep' = if (contains(projectNames, 'file-upload')) {
  name: 'fileUploadModule'
  scope: resourceGroup
  params: {
    environment: environment
    location: location
    appServicePlanId: sharedModule.outputs.appServicePlanId
    storageAccountConnectionString: sharedModule.outputs.storageAccountConnectionString
    fileUploadB2C_Instance: fileUploadB2C_Instance
    fileUploadB2C_Domain: fileUploadB2C_Domain
    fileUploadB2C_ClientId: fileUploadB2C_ClientId
    fileUploadB2C_ClientSecret: fileUploadB2C_ClientSecret
    fileUploadB2C_PasswordResetPolicyId: fileUploadB2C_PasswordResetPolicyId
    fileUploadB2C_SignUpSignInPolicyId: fileUploadB2C_SignUpSignInPolicyId
    fileUploadB2C_Tenant: fileUploadB2C_Tenant
  }
}

module todoModule './modules/todo.bicep' = if (contains(projectNames, 'todo')) {
  name: 'todoModule'
  scope: resourceGroup
  params: {
    environment: environment
    location: location
    appServicePlanId: sharedModule.outputs.appServicePlanId
    storageAccountConnectionString: sharedModule.outputs.storageAccountConnectionString
  }
}
