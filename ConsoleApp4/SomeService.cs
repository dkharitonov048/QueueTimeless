using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class SomeService : ISomeService
    {
        public async Task SomeMethod()
        {
            await Task.Yield();
            Console.WriteLine("Text from some method");
        }
    }
}
