using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var queue = new QueueTimeless<int>();
            for (int i = 0; i < 10; i++)
            {
                queue.Add(i);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            for (int i = 0; i < 10; i++)
            {
                queue.Add(i);
            }

            await queue.StopAsync();
            queue.Dispose();

            Console.WriteLine("Hello World!");
        }
    }

    public class QueueTimeless<TItem> : IDisposable
    {
        private readonly BlockingCollection<Packet<TItem>> _internalPipline;
        private Task _monitor;
        private CancellationTokenSource _cancellation;
        private Packet<TItem> _currentPacket;
        private readonly object _syncRoot = new object();

        public QueueTimeless()
        {
            _internalPipline = new BlockingCollection<Packet<TItem>>();
            _cancellation = new CancellationTokenSource();
            _currentPacket = new Packet<TItem>(3);
            _monitor = StartQueue();
        }

        public void Add(TItem item)
        {
            if (!_currentPacket.TryAdd(item))
            {
                lock (_syncRoot)
                {
                    if (!_currentPacket.TryAdd(item))
                    {
                        var tmp = SwitchPacketNoLock();
                        _internalPipline.Add(tmp);
                        _currentPacket.TryAdd(item);
                    }
                }
            }
        }

        private async Task StartQueue()
        {
            await Task.Yield();

            while (!_cancellation.IsCancellationRequested)
            {
                if (_internalPipline.TryTake(out var packet, TimeSpan.FromSeconds(1)))
                {
                    SendPacket(packet);
                }
                else
                {
                    if (_currentPacket.HasItems)
                    {
                        lock (_syncRoot)
                        {
                            if (_currentPacket.HasItems)
                            {
                                var tmp = SwitchPacketNoLock();
                                _internalPipline.Add(tmp);
                            }
                        }
                    }
                }
            }
        }

        private Packet<TItem> SwitchPacketNoLock()
        {
            return Interlocked.Exchange(ref _currentPacket, new Packet<TItem>(3));
        }

        private void SendPacket(Packet<TItem> packet)
        {
            Console.WriteLine($"Send packet. Packet size {packet.Size}, Items: {string.Join(',', packet.Items.Select(i => i.ToString()))}");
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _internalPipline?.Dispose();
        }

        public async Task StopAsync()
        {
            _cancellation.Cancel();
            await _monitor;
        }

        private class Packet<TItem>
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
}
