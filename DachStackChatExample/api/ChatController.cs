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
using Microsoft.AspNetCore.SignalR;
using DachStackApp.Hubs;

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
    [Route("api/chatHub")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _containerName;
        private IHubContext<ChatHub> _hubContext;
        public ChatController(IHubContext<ChatHub> hubContext, IConfiguration configuration, string containerName = "dach-file-controllerf3fef90e-a7c9-4242-b679-97517997e66d")
        {
            _configuration = configuration;
            _containerName = containerName;
            _configuration["AzureAccountName"] = "devstoreaccount1";
            _configuration["AzureAccountKey"] = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
            _hubContext = hubContext;
            Console.WriteLine($@"{_configuration.ToString()}");
            string storageConnectionString = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            Console.WriteLine($"Using Container: {_containerName}");
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromForm] string user, [FromForm] string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
            return Ok();
        }

    }
}