using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace DachStackApp.api
{
    [ApiController]
    [Route("api/file")]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        public UploadController(IConfiguration configuration, string containerName = "dach-file-controllerf3fef90e-a7c9-4242-b679-97517997e66d")
        {
            _configuration = configuration;
            _containerName = containerName;
            Console.WriteLine($@"{_configuration.ToString()}");
            string storageConnectionString = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            _blobServiceClient = new BlobServiceClient(storageConnectionString);

            Console.WriteLine($"Account Name: {_blobServiceClient.AccountName}");
            try
            {
                var container = _blobServiceClient.CreateBlobContainer(_containerName);
                Console.WriteLine($"Container: {container?.Value.Uri.ToString()??"Container Creation Failed"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
           
        }

        [HttpGet("get-presigned-url")]
        public IActionResult GetPresignedUrl(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(15));

            return Ok(new { url = sasUri.ToString() });
        }

        [HttpGet("get-presigned-url-for-block")]
        public IActionResult GetPresignedUrlForBlock(string filename, string blockid)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Write);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_configuration["AzureAccountName"], _configuration["AzureAccountKey"]));

            var uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken.ToString()
            };

            // Append block ID to the query
            uriBuilder.Query += $"&comp=block&blockid={Uri.EscapeDataString(blockid)}";

            return Ok(new { url = uriBuilder.Uri.ToString() });
        }

        [HttpGet("get-commit-url")]
        public IActionResult GetCommitUrl(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Write);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_configuration["AzureAccountName"], _configuration["AzureAccountKey"]));

            var uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken.ToString()
            };

            // Append block list commit operation to the query
            uriBuilder.Query += "&comp=blocklist";

            return Ok(new { url = uriBuilder.Uri.ToString() });
        }
    }
}