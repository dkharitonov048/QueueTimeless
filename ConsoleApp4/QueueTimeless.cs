using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class QueueTimeless<TItem> : IDisposable
    {
        private readonly BlockingCollection<Packet<TItem>> _internalPipline;
        private Task _monitor;
        private CancellationTokenSource _cancellation;
        private Packet<TItem> _currentPacket;
        private readonly object _syncRoot = new object();
        private readonly ISomeService _someService;

        private const int maxSize = 3;
        private const int timeInterval = 10;

        public QueueTimeless(ISomeService someService)
        {
            _someService = someService;
            _internalPipline = new BlockingCollection<Packet<TItem>>();
            _cancellation = new CancellationTokenSource();
            _currentPacket = new Packet<TItem>(maxSize);
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

        public IEnumerable<TItem> Items => _currentPacket.Items;

        private async Task StartQueue()
        {
            await Task.Yield();

            while (!_cancellation.IsCancellationRequested)
            {
                if (_internalPipline.TryTake(out var packet, TimeSpan.FromSeconds(timeInterval)))
                {
                    await SendPacket(packet);
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
            return Interlocked.Exchange(ref _currentPacket, new Packet<TItem>(maxSize));
        }

        private async Task SendPacket(Packet<TItem> packet)
        {
            await _someService.SomeMethod();
            //Console.WriteLine($"Send packet. Packet size {packet.Size}, Items: {string.Join(',', packet.Items.Select(i => i.ToString()))}");
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

        


    }
}
