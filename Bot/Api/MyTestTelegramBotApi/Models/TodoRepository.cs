using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTestTelegramBotApi.Models
{
    public class TodoRepository : ITodoRepository
    {
        public static ConcurrentDictionary<string, TodoItem> _todos =
            new ConcurrentDictionary<string, TodoItem>();

        public TodoRepository()
        {
            Add(new TodoItem { Name = "Item1" });
        }

        public void Add(TodoItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            item.Key = Guid.NewGuid().ToString();
            _todos[item.Key] = item;
        }

        public TodoItem Find(string key)
        {
            _todos.TryGetValue(key, out TodoItem item);
            return item;
        }

        public IEnumerable<TodoItem> GetAll()
            => _todos.Values;

        public TodoItem Remove(string key)
        {
            _todos.TryGetValue(key, out TodoItem item);
            _todos.TryRemove(key, out item);
            return item;
        }

        public void Update(TodoItem item)
        {
            _todos[item.Key] = item;
        }
    }
}
