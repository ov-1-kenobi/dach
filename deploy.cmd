@echo off
setlocal

REM Check arguments
if "%~1"=="" (
    echo Usage: deploy.cmd <environment> <appName> <subscription>
    exit /b 1
)
if "%~2"=="" (
    echo Usage: deploy.cmd <environment> <appName> <subscription>
    exit /b 1
)
if "%~3"=="" (
    echo Usage: deploy.cmd <environment> <appName> <subscription>
    exit /b 1
)

set ENVIRONMENT=%~1
set APPNAME=%~2
set SUBSCRIPTION=%~3

REM Set subscription
echo Setting Azure subscription to %SUBSCRIPTION%
az account set --subscription "%SUBSCRIPTION%"
if errorlevel 1 (
    echo Failed to set subscription
    exit /b 1
)

REM Variables
REM Adjust PROJECT_DIR if needed. Assuming current directory is project root.
set PROJECT_DIR=%~dp0
set PROJECT_DIR=%PROJECT_DIR:~0,-1%
REM Assuming the .NET project is located at %PROJECT_DIR%\%APPNAME%
set PROJECT_PATH=%PROJECT_DIR%\%APPNAME%
set PUBLISH_FOLDER=%PROJECT_PATH%\bin\Release\net8.0\publish
set APP_ZIP=%PROJECT_PATH%\app.zip
set INFRASTRUCTURE_DIR=%PROJECT_DIR%\infrastructure

REM Build and publish .NET project
echo Building and publishing .NET project...
cd "%PROJECT_PATH%"
dotnet publish -c Release
if errorlevel 1 (
    echo dotnet publish failed
    exit /b 1
)

if not exist "%PUBLISH_FOLDER%" (
    echo Publish folder not found at %PUBLISH_FOLDER%
    exit /b 1
)

REM Create app.zip
echo Creating app.zip from published output...
cd "%PUBLISH_FOLDER%"
if exist app.zip del app.zip
tar -a -c -f app.zip *
if errorlevel 1 (
    echo Failed to create app.zip with tar
    exit /b 1
)
move app.zip "%PROJECT_PATH%\"

REM Deploy Infrastructure with Bicep
cd "%INFRASTRUCTURE_DIR%"
echo Deploying Bicep templates...
az deployment sub create ^
  --name DACHDeployment-%ENVIRONMENT% ^
  --location eastus ^
  --template-file main.bicep ^
  --parameters @parameters\%ENVIRONMENT%.parameters.json ^
  --parameters projectNames='["%APPNAME%"]'
if errorlevel 1 (
    echo Bicep deployment failed
    exit /b 1
)

REM App service name could be pattern-based (e.g. DACH-%APPNAME%-%ENVIRONMENT%)
set WEBAPPNAME=DACH-%APPNAME%-%ENVIRONMENT%

REM Deploy the code to Web App
cd "%PROJECT_PATH%"
echo Deploying app.zip to Web App %WEBAPPNAME%...
az webapp deploy ^
  --resource-group DACH-%ENVIRONMENT%-rg ^
  --name %WEBAPPNAME% ^
  --src-path app.zip ^
  --type zip
if errorlevel 1 (
    echo Web app deploy failed
    exit /b 1
)

echo Deployment completed successfully!
endlocal
