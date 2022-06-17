using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

public static class NamedTypeSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetHierarchy(this INamedTypeSymbol symbol)
    {
        var types = new List<INamedTypeSymbol>();

        var currentType = symbol;

        while (currentType!.ToString() != "object")
        {
            types.Add(currentType);
            if (currentType.BaseType == null)
            {
                break;
            }

            currentType = currentType.BaseType;
        }

        types.Reverse();

        return symbol.AllInterfaces.AddRange(types);
    }

    public static IEnumerable<IMethodSymbol> GetAllPublicMethods(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetMembers().Where(member => member.Kind == SymbolKind.Method && member.DeclaredAccessibility == Accessibility.Public).Cast<IMethodSymbol>().Where(member => member.MethodKind == MethodKind.Ordinary));
    }

    public static IEnumerable<AttributeData> GetAllAttributes(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetAttributes());
    }
}