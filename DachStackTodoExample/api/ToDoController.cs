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
    public static class StorageTableExtensions
    {
        public static ToDoItem getObjectValue(this StorageEntity storageEntity)
        {
            return storageEntity.JsonValue == null ? null : JsonSerializer.Deserialize<ToDoItem>(storageEntity.JsonValue);
        }
        public static void setObjectValue(this StorageEntity storageEntity, ToDoItem value)
        {
            storageEntity.JsonValue = JsonSerializer.Serialize(value);
        }

    }
    public class StorageEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public float Version { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string JsonValue { get; set; }
        
    }
    public class ToDoItem
    {
        public string? Id { get; set; }
        public required string Task { get; set; }
        //public required string Description { get; set; }
        public bool IsComplete { get; set; }
    }
    [ApiController]
    [Route("api/todo")]
    public class ToDoController : ControllerBase
    {
        public const string APARTITIONKEY = "e7c6ce3d-091a-4d60-9901-c1257cc3146b";
        private static List<ToDoItem> _items = new List<ToDoItem>();
        private readonly IConfiguration _configuration;
        private readonly TableServiceClient _TableServiceClient;
        private readonly string _tableName;

    public ToDoController(IConfiguration configuration, string tableName = "devtodostorage")
    {
        _configuration = configuration;
        _tableName = tableName;

        string storageConnectionString = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        _TableServiceClient = new TableServiceClient(storageConnectionString);

    }

        [HttpGet]
        public IActionResult GetItems() 
        { 
            _TableServiceClient.CreateTableIfNotExists(_tableName);
            var retHTML = $"";
            TableClient tableClient = _TableServiceClient.GetTableClient(_tableName);
            var items = tableClient.Query<StorageEntity>();

            var todos = items.Select(t => t.getObjectValue() as ToDoItem).ToList();

            foreach (StorageEntity item in items)
            {
                retHTML += $"<li id='todo-{item.getObjectValue().Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.getObjectValue().Task}</span><div><button hx-delete='/api/todo/{item.getObjectValue().Id}' hx-target='closest li' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button><button hx-target='closest li' hx-swap='outerHTML' hx-patch='/api/todo/{item.getObjectValue().Id}' hx-vals='{{\"IsComplete\":true}}' class='btn btn-success btn-xs'>Complete</button></div></li>";
            }
            return Ok(retHTML);
        }

        [HttpPost]
        public IActionResult AddItem([FromForm]ToDoItem item)
        {
            _TableServiceClient.CreateTableIfNotExists(_tableName);
            TableClient tableClient = _TableServiceClient.GetTableClient(_tableName);
            //TODO:KO; tie in principal info here for 'logging' changers
            string entityId = Guid.NewGuid().ToString();
            StorageEntity storageEntity = new StorageEntity() {
                PartitionKey = APARTITIONKEY, //principal tenant info
                RowKey = entityId, //UID; int for now
                //ObjectValue = item
            };
            item.Id = entityId;
            storageEntity.setObjectValue(item); 
            // item.Id = _items.Count + 1;
            // _items.Add(item);
            tableClient.AddEntity<StorageEntity>(storageEntity);
            
            var retHTML = $"""
            <li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'>
            <span>{item.Task}</span>
            <div>
            <button hx-delete='/api/todo/{item.Id}' hx-target='#todo-{item.Id}' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button>
            <button hx-patch='/api/todo/{item.Id}' hx-vals='\"IsComplete\":true' class='btn btn-success btn-xs'>Complete</button></div></li>
            """;
            if (Request.Headers["Accept"].ToString().Contains("*/*"))
            {
                return GetItems();
                //return Content(retHTML, "text/html");
            }
            else
            {
                return new JsonResult(new { Success = true, Html = retHTML });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItem(string id)
        {
            _TableServiceClient.CreateTableIfNotExists(_tableName);
            TableClient tableClient = _TableServiceClient.GetTableClient(_tableName);
            StorageEntity cadaver = tableClient.GetEntity<StorageEntity>(APARTITIONKEY, id);
            if (cadaver != null)
            {
                tableClient.DeleteEntity(cadaver);
                return Ok();
            }

            return NotFound();

        }

        [HttpPatch("{id}")]
        public IActionResult UpdateItem(string id, ToDoItem updatedItem)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            item.IsComplete = updatedItem.IsComplete;

            var retHTML = $"<li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.Task}</span><div><button hx-delete='/api/todo/{item.Id}' hx-target='#todo-{item.Id}' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button></div></li>";

            return Ok(retHTML);
        }
    }

}
