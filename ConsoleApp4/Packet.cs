using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp4
{
    public class Packet<TItem>
    {
        private readonly ConcurrentQueue<TItem> _queue;
        private readonly int _maxSize;

        public Packet(int maxSize)
        {
            _maxSize = maxSize;
            _queue = new ConcurrentQueue<TItem>();
        }

        public bool HasItems => _queue.Count > 0;

        public bool IsFull => _queue.Count + 1 > _maxSize;

        public int Size => _queue.Count;

        public IEnumerable<TItem> Items => _queue;

        public bool TryAdd(TItem item)
        {
            if (IsFull)
            {
                return false;
            }
            _queue.Enqueue(item);
            return true;
        }

    }
}
