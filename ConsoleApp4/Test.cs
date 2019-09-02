using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    [TestFixture]
    public class Test
    {
        [TestCase]
        public async Task TestMethod()
        {
            var service = new Mock<ISomeService>();
            service.Setup(m => m.SomeMethod())
                .Returns(async () => { await Task.Yield(); Console.WriteLine("Text from mock object"); });

            var queueTimeless = new QueueTimeless<int>(service.Object);

            for (int i = 0; i < 10; i++)
            {
                queueTimeless.Add(i);
            }

            var queueSize = queueTimeless.Items.Count();

            Assert.AreEqual(queueSize, 1);

            await Task.Delay(TimeSpan.FromSeconds(11));

            queueSize = queueTimeless.Items.Count();

            Assert.AreEqual(queueSize, 0);

            await Task.Delay(TimeSpan.FromMinutes(5));


            await queueTimeless.StopAsync();
            queueTimeless.Dispose();

            Console.ReadKey();

        }
    }
}
