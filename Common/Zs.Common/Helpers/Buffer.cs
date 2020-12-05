using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Zs.Common.Helpers
{
    public class Buffer<T>
    {
        private readonly ConcurrentQueue<T> _buffer;

        public bool IsEmpty => _buffer.IsEmpty;

        public delegate void BufferChangedDelegate(object sender, T item);

        public event BufferChangedDelegate OnEnqueue;
        public event BufferChangedDelegate OnDequeue;


        public Buffer()
        {
            _buffer = new ConcurrentQueue<T>();
        }

        public Buffer(IEnumerable<T> collection)
        {
            _buffer = new ConcurrentQueue<T>(collection);
        }


        public void Enqueue(T item)
        {
            _buffer.Enqueue(item);
            OnEnqueue?.Invoke(this, item);
        }

        public bool TryDequeue(out T item)
        {
            bool result = _buffer.TryDequeue(out item);

            if (result)
                OnDequeue?.Invoke(this, item);

            return result;
        }

    }
}
