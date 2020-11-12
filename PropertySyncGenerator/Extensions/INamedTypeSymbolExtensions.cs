using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PropertySyncGenerator.Extensions
{
    public static class INamedTypeSymbolExtensions
    {
        private static readonly ConcurrentDictionary<INamedTypeSymbol, IEnumerable<IPropertySymbol>> s_propertyMap = new ConcurrentDictionary<INamedTypeSymbol, IEnumerable<IPropertySymbol>>(SymbolEqualityComparer.Default);

        public static IEnumerable<IPropertySymbol> GetAccessibleProperties(this INamedTypeSymbol symbol)
        {
            if (!s_propertyMap.ContainsKey(symbol))
            {
                s_propertyMap[symbol] = symbol.EnumerateAccessibleProperties().ToArray();
            }

            return s_propertyMap[symbol];
        }

        private static IEnumerable<IPropertySymbol> EnumerateAccessibleProperties(this INamedTypeSymbol symbol)
        {
            INamedTypeSymbol? toGet = symbol;
            while (toGet is { })
            {
                foreach (IPropertySymbol item in toGet.GetMembers()
                                                       .OfType<IPropertySymbol>()
                                                       .Where(x => !x.IsReadOnly
                                                           && x.SetMethod is { }
                                                           && x.SetMethod.DeclaredAccessibility == Accessibility.Public
                                                           && x.GetMethod is { }
                                                           && x.GetMethod.DeclaredAccessibility == Accessibility.Public
                                                           && !x.SetMethod.IsInitOnly
                                                           && !x.IsIndexer
                                                           && !x.IsStatic
                                                           && x.Type.TypeKind != TypeKind.Pointer
                                                           && !x.IsOverride
                                                           && !x.IsObsolete()))
                {
                    yield return item;
                }

                if (toGet.BaseType is null || toGet.BaseType.Name == "Object")
                {
                    toGet = null;
                }
                else
                {
                    toGet = toGet.BaseType;
                }
            }
        }

        public static bool HasMatchingProperties(this INamedTypeSymbol left, INamedTypeSymbol right)
        {
            foreach (IPropertySymbol item in left.GetAccessibleProperties())
            {
                if (right.GetAccessibleProperties().Any(x => x.Name == item.Name && SymbolEqualityComparer.Default.Equals(x.Type, item.Type)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
