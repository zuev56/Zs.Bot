using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTestTelegramBotApi.Models;

namespace MyTestTelegramBotApi.Controllers
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

            return new ObjectResult(item);
        }

    }
}
