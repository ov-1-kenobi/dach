@echo off
setlocal

if "%~1"=="" (
    echo Usage: teardown.cmd <environment> [appName] <subscription>
    exit /b 1
)

set ENVIRONMENT=%~1
set APPNAME=%~2
set SUBSCRIPTION=%~3

REM If subscription is provided as the third arg, set it:
if not "%SUBSCRIPTION%"=="" (
    az account set --subscription "%SUBSCRIPTION%"
    if errorlevel 1 (
        echo Failed to set subscription
        exit /b 1
    )
)

REM Resource group naming pattern: DACH-<env>-rg
set RGNAME=DACH-%ENVIRONMENT%-rg

echo Deleting resource group %RGNAME%
az group delete --name %RGNAME% --yes --no-wait
if errorlevel 1 (
    echo Failed to delete resource group %RGNAME%
    exit /b 1
)

echo Resource group deletion initiated. This may take some time.
endlocal
