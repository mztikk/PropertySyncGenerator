using System.Runtime.CompilerServices;

namespace PropertySyncGenerator
{
    internal static class CommonTypes
    {
        public const string StringDict = "System.Collections.Generic.Dictionary<string, string>";
        public const string StringHashSet = "System.Collections.Generic.HashSet<string>";
        public const string StringICollection = "System.Collections.Generic.ICollection<string>";


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EmptyArray(in string type) => $"System.Array.Empty<{type}>";
    }
}
