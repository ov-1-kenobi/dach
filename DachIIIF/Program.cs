using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Microsoft.AspNetCore.SignalR.Client;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var configuration = builder.Configuration;

string blobStorageConnectionString = configuration["BlobStorageConnectionString"];
builder.Services.AddSingleton(new BlobServiceClient(blobStorageConnectionString));

string tableStorageConnectionString = configuration["TableStorageConnectionString"]; 
builder.Services.AddSingleton(new TableServiceClient(tableStorageConnectionString));

string signalRConnectionString = configuration["SignalRConnectionString"]; 
var hubConnection = new HubConnectionBuilder() 
    .WithUrl(signalRConnectionString) 
    .Build(); 
builder.Services.AddSingleton(hubConnection);

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();



builder.Build().Run();
