<img src="https://github.com/user-attachments/assets/f63c4554-f000-47ff-a78f-8dcf2a236ad8" alt="Dach Logo" width="128" height="128">

#  CHAT Example
cd DachStackChatExample
dotnet publish -c Release
cd bin\Release\net8.0\publish
tar -a -c -f app.zip *
cd ..
cd infrastructure

az deployment sub create --name DachManualDeployment-002-uat --location eastus --template-file main.bicep --parameters @parameters\uat.parameters.json --parameters projectNames="['chat',]"
cd ..
cd DachStackChatExample
cd bin\Release\net8.0\publish

az webapp deploy --resource-group DACH-uat-rg --name DACH-chat-uat --src-path .\app.zip --type zip

az webapp show --resource-group DACH-uat-rg --name DACH-chat-uat --query defaultHostName -o tsv

