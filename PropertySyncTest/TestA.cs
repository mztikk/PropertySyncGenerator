using PropertySyncGenerator;

namespace PropertySyncTest
{
    [Syncable]
    public class TestA
    {
        public string StringA { get; set; }
        public string StringB { get; set; }
        public int IntA { get; set; }
        public bool BoolA { get; set; }
        public bool BoolB { get; set; }
    }

    [Syncable]
    public class Test2A : TestA
    {
        public string String2A { get; set; }
    }
}
