using System;
using System.Collections.Generic;

namespace PropertySyncTest
{
    public record TestRecordA()
    {
        public string StringA { get; set; }
        public string StringB { get; set; }
        public int IntA { get; set; }
        public int IntB { get; set; }
    }
    public record TestRecordB()
    {
        public string StringB { get; set; }
        public int IntB { get; set; }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            //HelloWorldGenerated.HelloWorldClass.HelloWorld();
            var a = new TestA() { StringA = "Hello from TestA!", IntA = 59 };
            var b = new TestB() { IntA = 6, StringA = "S", StringB = "B" };
            Console.WriteLine("Sync from TestA");
            //PropertySync.Sync(a, b);
            a.Sync(b);
            Console.WriteLine(b.StringA);
            Console.WriteLine(b.StringB);
            Console.WriteLine(b.IntA);

            Console.WriteLine();

            var dict = new Dictionary<string, string>() { {"StringB", "Hello from Dictionary!" } };
            Console.WriteLine("Sync from Dictionary<string, string>");
            //PropertySync.Sync(dict, b);
            dict.Sync(b);

            Console.WriteLine(b.StringA);
            Console.WriteLine(b.StringB);
            Console.WriteLine(b.IntA);

            Console.WriteLine();

            var ra = new TestRecordA() { StringB = "Hello from TestRecordA!", IntA = 59 };
            var rb = new TestRecordB() { IntB = 6, StringB = "B" };
            Console.WriteLine("Sync from TestRecordA");
            //PropertySync.Sync(ra, rb);
            ra.Sync(rb);
            Console.WriteLine(rb.StringB);
            Console.WriteLine(rb.IntB);

            Console.WriteLine();

            Console.WriteLine("Sync from Dictionary<string, string>");
            //PropertySync.Sync(dict, rb);
            dict.Sync(rb);
            Console.WriteLine(rb.StringB);
            Console.WriteLine(rb.IntB);
        }
    }
}
