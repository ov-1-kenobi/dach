using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Azure.Data;
using Azure.Data.Tables;
using Azure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;

namespace DachStackApp.api
{
    public class StorageEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public float Version { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
    public class UserStorageEntity : StorageEntity
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string PasswordHash { get; set; }
    }
    public class RoomStorageEntity : StorageEntity
    {
        //partition key comes from account
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class MessageStorageEntity : StorageEntity
    {
        //partition key denotes room
        public string Content { get; set; }
    }

    [ApiController]
    [Route("api/file")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        public UploadController(IConfiguration configuration, string containerName = "dach-file-controllerf3fef90e-a7c9-4242-b679-97517997e66d")
        {
            _configuration = configuration;
            _containerName = containerName;
            _configuration["AzureAccountName"] = "devstoreaccount1";
            _configuration["AzureAccountKey"] = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
            Console.WriteLine($@"{_configuration.ToString()}");
            string storageConnectionString = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            _blobServiceClient = new BlobServiceClient(storageConnectionString);
            var _blobContainerClient = new BlobContainerClient(storageConnectionString, _containerName);
            
            Console.WriteLine($"Account Name: {_blobServiceClient.AccountName}");
            try
            {
                if(!_blobContainerClient.Exists()) 
                {
                    var container = _blobServiceClient.CreateBlobContainer(_containerName);
                    Console.WriteLine($"Container: {container?.Value.Uri.ToString()??"Container Creation Failed"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            Console.WriteLine($"Using Container: {_containerName}");
        }
        [HttpGet("get-files")]
        public IActionResult GetFiles()
        {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var files = containerClient.GetBlobs();

                var retHTML = $"";
                foreach(var item in files)
                {
                    retHTML += $"""
                    <div id="file{item.GetUid()}" class="card card-compact bg-base-100 w-48 h-48 shadow-xl">
                        <figure class="h-48 w-full flex items-center justify-center bg-gray-200">
                            {renderItemContainer(containerClient, item)}
                        </figure>
                        <div class="card-body">
                            <h4 class="card-title truncate">{item.Name}</h4>
                            <div class="card-actions justify-end">
                                <button hx-get='/api/file/start-delete/?filename={item.Name}' hx-target='#confirm-modal-button' hx-trigger="click" hx-swap='innerHTML' class='btn btn-error btn-xs'>
                                    Delete
                                </button>
                                <button hx-swap='innerHTML' hx-get='/api/file/get-file/?filename={item.Name}' onclick="preview_modal.showModal();" hx-target='#modal_image_preview' class='btn btn-success btn-xs'>
                                    View
                                </button>
                            </div>
                        </div>
                    </div>
                    """;
                }

                return Ok(retHTML);
        }

        private string renderItemContainer(BlobContainerClient client, BlobItem item)
        {
            if (item.Name.EndsWith(".mp4") || item.Name.EndsWith(".mpeg"))
            {
                var vidVal = $"""
                    <video controls>
                        <source src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}"/>
                        Sorry, your browser busted
                    </video>
                """;
            
                return vidVal;
            }
            if (item.Name.EndsWith(".pdf"))
            {
                var docVal = $"""
                    <iframe src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}#page=1"></iframe>
                """;

                return docVal;
            }
            var imgVal = $"""<img class="h-full w-full object-cover" src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}"/>""";
            return imgVal;
        }

        [HttpGet("get-file")]
        public IActionResult GetFile(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(filename);
            var retVal = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15));
            var retHTML = $"""
                <img class='' src='{retVal.ToString()}' alt='No Image'/>
            """;
            return Ok(retHTML);
        }

        [HttpGet("start-delete")]
        public IActionResult StartDelete(string fileName)
        {
            var delModal = $"""
                <button type='submit' hx-delete='/api/file/?filename={fileName}' hx-target='#file{FileControllerHelpers.GetUid(fileName)}' hx-swap='delete' class='btn btn-error btn-xs'>
                    Delete
                </button>
            """;
            return Ok(delModal);
        }

        [HttpDelete()]
        public IActionResult DeleteFile(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(filename);
            blobClient.Delete();
            return Ok();
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
    public static class FileControllerHelpers
    {
        public static string GetUid(this BlobItem item)
        {
            return item.Name.GetHashCode().ToString("x");
        }
        public static string GetUid(string item_name)
        {
            return item_name.GetHashCode().ToString("x");
        }
    }
}