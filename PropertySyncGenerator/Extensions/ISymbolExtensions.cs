using System.Collections.Concurrent;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PropertySyncGenerator.Extensions
{
    public static class ISymbolExtensions
    {
        private const string ObsoleteAttribute = "ObsoleteAttribute";
        private static readonly ConcurrentDictionary<ISymbol, bool> s_obsoleteMap = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
        public static bool IsObsolete(this ISymbol symbol)
        {
            if (!s_obsoleteMap.ContainsKey(symbol))
            {
                s_obsoleteMap[symbol] = symbol.GetAttributes().Any(y => y.AttributeClass?.Name.Equals(ObsoleteAttribute) == true);
            }

            return s_obsoleteMap[symbol];
        }

        private const string SyncableAttribute = "PropertySyncGenerator.SyncableAttribute";
        private static readonly ConcurrentDictionary<ISymbol, bool> s_syncableMap = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
        public static bool IsSyncable(this ISymbol symbol)
        {
            if (!s_syncableMap.ContainsKey(symbol))
            {
                s_syncableMap[symbol] = symbol.GetAttributes().Any(y => y.AttributeClass?.ToString().Equals(SyncableAttribute) == true);
            }

            return s_syncableMap[symbol];
        }
    }
}
