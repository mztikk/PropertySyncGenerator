using System.Collections.Concurrent;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PropertySyncGenerator.Extensions
{
    public static class ITypeSymbolExtensions
    {
        private static readonly ConcurrentDictionary<ITypeSymbol, bool> s_symbolParseMap = new ConcurrentDictionary<ITypeSymbol, bool>(SymbolEqualityComparer.Default);

        public static bool HasStringParse(this ITypeSymbol symbol)
        {
            if (!s_symbolParseMap.ContainsKey(symbol))
            {
                s_symbolParseMap[symbol] = symbol.GetMembers()
                                                 .OfType<IMethodSymbol>()
                                                 .Any(x => x.Kind == SymbolKind.Method
                                                           && x.Name == "Parse"
                                                           && x.Parameters.Length == 1
                                                           && x.Parameters[0].Type.Name == "String");
            }

            return s_symbolParseMap[symbol];
        }
    }
}
