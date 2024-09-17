#r "nuget: Azure.Storage.Blobs, 12.21.2"

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

async Task SetCors()
{
    Console.WriteLine("CORS starting.");
    string connectionString = "UseDevelopmentStorage=true"; // Azurite default connection string
    Console.WriteLine("CORS settings" +  connectionString);
    BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
    Console.WriteLine("CORS " + blobServiceClient.Uri);
    BlobServiceProperties serviceProperties = await blobServiceClient.GetPropertiesAsync();
    Console.WriteLine("CORS properties");
    serviceProperties.Cors = new List<BlobCorsRule>
    {
        new BlobCorsRule
        {
            AllowedOrigins = "*",
            AllowedMethods = "GET,POST,PUT,DELETE",
            AllowedHeaders = "*",
            ExposedHeaders = "*",
            MaxAgeInSeconds = 3600
        }
    };
    Console.WriteLine("CORS setting properties" + serviceProperties.Cors.Count);
    await blobServiceClient.SetPropertiesAsync(serviceProperties);
    Console.WriteLine("CORS settings applied successfully.");
}

await SetCors();
