using System;
using System.Collections.Generic;

namespace PropertySyncTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //HelloWorldGenerated.HelloWorldClass.HelloWorld();
            var a = new TestA() { StringA = "Hello from TestA!", IntA = 59 };
            var b = new TestB() { IntA = 6, StringA = "S", StringB = "B" };
            Console.WriteLine("Sync from TestA");
            PropertySync.Sync(a, b);
            Console.WriteLine(b.StringA);
            Console.WriteLine(b.StringB);
            Console.WriteLine(b.IntA);

            var dict = new Dictionary<string, string>() { {"StringB", "Hello from Dictionary" } };
            Console.WriteLine("Sync from Dictionary<string, string>");
            PropertySync.Sync(dict, b);
            Console.WriteLine(b.StringA);
            Console.WriteLine(b.StringB);
            Console.WriteLine(b.IntA);
        }
    }
}
