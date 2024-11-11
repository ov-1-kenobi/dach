// infrastructure/main.bicep

param environment string
param location string = resourceGroup().location
param projectNames array = []

module sharedModule './modules/shared.bicep' = {
  name: 'sharedResources'
  params: {
    environment: environment
    location: location
  }
}

module chatModule './modules/chat.bicep' = if(contains(projectNames, 'chat')) {
  name: 'chatModule'
  params: {
    environment: environment
    location: location
    sharedResources: sharedModule.outputs
  }
}

module fileUploadModule './modules/fileUpload.bicep' = if(contains(projectNames, 'file-upload')) {
  name: 'fileUploadModule'
  params: {
    environment: environment
    location: location
    sharedResources: sharedModule.outputs
  }
}

module todoModule './modules/todo.bicep' = if(contains(projectNames, 'todo')) {
  name: 'todoModule'
  params: {
    environment: environment
    location: location
    sharedResources: sharedModule.outputs
  }
}
