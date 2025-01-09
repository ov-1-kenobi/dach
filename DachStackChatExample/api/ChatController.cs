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
using Azure.Storage.Blobs;

namespace DachStackApp.api
{
    public class StorageEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Lobby"; //put 'room' into here for a quick separation
        public string RowKey { get; set; } = "";
        public float Version { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
    public class MessageEntity : StorageEntity
    {
        public string UserName { get; set; }
        public string Message { get; set; }
    }

    // public class UserStorageEntity : StorageEntity
    // {
    //     public string UserName { get; set; }
    //     public string Password { get; set; }
    //     public string Salt { get; set; }
    //     public string PasswordHash { get; set; }
    // }
    // public class RoomStorageEntity : StorageEntity
    // {
    //     //partition key comes from account
    //     public string Name { get; set; }
    //     public string Description { get; set; }
    // }
    // public class MessageStorageEntity : StorageEntity
    // {
    //     //partition key denotes room
    //     public string Content { get; set; }
    // }

    [ApiController]
    [Route("api/chatHub")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _tableName;
        private TableServiceClient _tableServiceClient;
        private IHubContext<ChatHub> _hubContext;
        public ChatController(IHubContext<ChatHub> hubContext, IConfiguration configuration, 
        BlobServiceClient blobServiceClient, TableServiceClient tableServiceClient, 
        string tableName = "devchatstorage")
        {
            _tableServiceClient = tableServiceClient;
            
            _configuration = configuration;
            _tableName = tableName;
            _hubContext = hubContext;
            _tableServiceClient.CreateTableIfNotExists(_tableName);

        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromForm] string user, [FromForm] string message, [FromForm] string room)
        {
            //write message to the table
            TableClient tableClient = _tableServiceClient.GetTableClient(_tableName);
            string entityId = Guid.NewGuid().ToString();
            MessageEntity messageEntity = new MessageEntity() {
                PartitionKey = room,
                RowKey = entityId,
                Timestamp = DateTime.UtcNow,
                UserName = user,
                Message = message,
            };
            
            tableClient.AddEntity<MessageEntity>(messageEntity);
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
            return Ok();
        }
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string username)
        {
            //user would come from principal info from auth/jwt
            //await get messages from table
            var retHTML = $"";
            TableClient tableClient = _tableServiceClient.GetTableClient(_tableName);
            var items = tableClient.Query<MessageEntity>();
            foreach (MessageEntity item in items.OrderBy(me => me.Timestamp))
            {
                retHTML += $"""
                <li id='chat{item.RowKey}' class='chat {((item.UserName==username) ? "chat-end" : "chat-start")} mb-2'>
                    <div class='chat-bubble'>
                        <strong>{item.UserName}:</strong> {item.Message}
                    </div>
                </li>
                """;
            }
            return Ok(retHTML);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage([FromForm] string message)
        {

            return Ok(); 
        }
    }
}