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

            var test = new Test();
            await test.TestMethod();
            //var queue = new QueueTimeless<int>(new SomeService());
            //for (int i = 0; i < 10; i++)
            //{
            //    queue.Add(i);
            //    await Task.Delay(TimeSpan.FromSeconds(2));
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    queue.Add(i);
            //}

            //await queue.StopAsync();
            //queue.Dispose();

            //Console.WriteLine("Hello World!");
        }
    }

   
}
