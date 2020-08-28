using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTestAPI.Models;

namespace MyTestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : Controller
    {
        public ITodoRepository TodoItems { get; set; }

        public TodoController(ITodoRepository todoItems)
        {
            TodoItems = todoItems;
        }

        public IEnumerable<TodoItem> GetAll()
            => TodoItems.GetAll();

        [HttpGet("{id}", Name = "GetTodo")]
        public IActionResult GetById(string id)
        {
            var item = TodoItems.Find(id);

            if (item == null)
            {
                return NotFound();
            }

            //200
            return new ObjectResult(item);
        }

        [HttpPost]
        public IActionResult Create([FromBody] TodoItem item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            TodoItems.Add(item);

            //201
            return CreatedAtRoute("GetToDo", new { id = item.Key }, item);
        }

        [HttpPut("{id}")]
        public IActionResult Update(string id, [FromBody] TodoItem item)
        {
            if (item == null || item.Key != id)
            {
                return BadRequest();
            }

            if (TodoItems.Find(id) is null)
            {
                return NotFound();
            }

            TodoItems.Update(item);

            //204
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            TodoItems.Remove(id);
            
            //204
        }

    }
}
