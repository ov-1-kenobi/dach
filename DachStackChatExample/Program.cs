using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using DachStackApp.api;
using DachStackApp;
using DachStackApp.Hubs;
using System.Reflection.Metadata;
using System.ComponentModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Data.Tables;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null;});
builder.Services.AddSignalR();

if (environmentName == "Development")
{
    builder.Services.AddSingleton(new BlobServiceClient("UseDevelopmentStorage=true"));
    builder.Services.AddSingleton(new TableServiceClient("UseDevelopmentStorage=true"));
}
else
{
    var storageConnectionString = configuration["StorageAccountConnectionString"];
    builder.Services.AddSingleton(new BlobServiceClient(storageConnectionString));
    builder.Services.AddSingleton(new TableServiceClient(storageConnectionString));

    var signalRConnectionString = configuration["AzureSignalRConnectionString"];
    builder.Services.AddSignalR().AddAzureSignalR(signalRConnectionString);
}

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

/** setup to use static/htmx content from wwwroot **/
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();
