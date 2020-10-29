using System.Linq;
using Microsoft.CodeAnalysis;

namespace PropertySyncGenerator.Extensions
{
    public static class ITypeSymbolExtensions
    {
        public static bool HasStringParse(this ITypeSymbol symbol) => symbol.GetMembers()
                                                                      .OfType<IMethodSymbol>()
                                                                      .Any(x => x.Kind == SymbolKind.Method
                                                                                && x.Name == "Parse"
                                                                                && x.Parameters.Length == 1
                                                                                && x.Parameters[0].Type.Name == "String");
    }
}
