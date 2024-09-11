using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace DachStackApp.api
{
    public class FileItem
    {
        public int Id { get; set; }
        public required string Task { get; set; }
        public bool IsComplete { get; set; }
    }
    [ApiController]
    [Route("api/todo")]
    public class FileController : ControllerBase
    {
        private static List<FileItem> _items = new List<FileItem>();

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
        public IActionResult AddItem([FromForm]FileItem item)
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
        public IActionResult UpdateItem(int id, FileItem updatedItem)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();

            item.IsComplete = updatedItem.IsComplete;

            var retHTML = $"<li id='todo-{item.Id}' class='flex items-center justify-between bg-white p-3 rounded shadow'><span>{item.Task}</span><div><button hx-delete='/api/todo/{item.Id}' hx-target='#todo-{item.Id}' hx-swap='outerHTML' class='btn btn-error btn-xs'>Delete</button></div></li>";

            return Ok(retHTML);
        }
    }

}
