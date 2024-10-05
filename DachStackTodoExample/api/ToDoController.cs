using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Azure.Data;
using Azure.Data.Tables;

namespace DachStackApp.api
{
    public class ToDoItem
    {
        public int Id { get; set; }
        public required string Task { get; set; }
        public required string Description { get; set; }
        public bool IsComplete { get; set; }
    }
    [ApiController]
    [Route("api/todo")]
    public class ToDoController : ControllerBase
    {
        private static List<ToDoItem> _items = new List<ToDoItem>();
        private readonly IConfiguration _configuration;
        private readonly TableServiceClient _TableServiceClient;
        private readonly string _tableName;
    public ToDoController(IConfiguration configuration, string tableName = "dach-table-controllerf3fef90e-a7c9-4242-b679-97517997e66e")
    {
        _configuration = configuration;
        _tableName = tableName;

        string storageConnectionString = @"DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
        _TableServiceClient = new TableServiceClient(storageConnectionString);

        _TableServiceClient.CreateTableIfNotExists(_tableName);
        var tableClient = _TableServiceClient.GetTableClient(_tableName);
        
    }

        [HttpGet]
        public IActionResult GetItems() 
        { 
            var retHTML = $"";
            foreach(var item in _items)
            {
                retHTML += $"<li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.Task}</span><div><button hx-delete='/api/todo/{item.Id}' hx-target='closest li' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button><button hx-target='closest li' hx-swap='outerHTML' hx-patch='/api/todo/{item.Id}' hx-vals='{{\"IsComplete\":true}}' class='btn btn-success btn-xs'>Complete</button></div></li>";
            }
            return Ok(retHTML);
        }

        [HttpPost]
        public IActionResult AddItem([FromForm]ToDoItem item)
        {
            item.Id = _items.Count + 1;
            _items.Add(item);
            var retHTML = $"<li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.Task}</span><div><button hx-delete='/api/todo/{item.Id}' hx-target='#todo-{item.Id}' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button><button hx-patch='/api/todo/{item.Id}' hx-vals='{{\"IsComplete\":true}}' class='btn btn-success btn-xs'>Complete</button></div></li>";
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
        public IActionResult DeleteItem(int id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            _items.Remove(item);
            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateItem(int id, ToDoItem updatedItem)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            item.IsComplete = updatedItem.IsComplete;

            var retHTML = $"<li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.Task}</span><div><button hx-delete='/api/todo/{item.Id}' hx-target='#todo-{item.Id}' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button></div></li>";

            return Ok(retHTML);
        }
    }

}
