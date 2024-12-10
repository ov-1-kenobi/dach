// infrastructure/main.bicep

targetScope = 'subscription'

param environment string
param location string = 'eastus'  // Default location
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
    functionAppServicePlanId: sharedModule.outputs.functionAppServicePlanId
    storageAccountConnectionString: sharedModule.outputs.storageAccountConnectionString
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
