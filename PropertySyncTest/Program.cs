using System;

namespace PropertySyncTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //HelloWorldGenerated.HelloWorldClass.HelloWorld();
            var a = new TestA() { StringA = "Hello from TestA", IntA = 5 };
            var b = new TestB() { IntA = 6, StringA = "S", StringB = "B" };
            PropertySync.Sync(a, b);
            Console.WriteLine(b.StringA);
            Console.WriteLine(b.StringB);
            Console.WriteLine(b.IntA);
        }
    }
}
