using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.CodeAnalysis;

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

        //types.Reverse();

        //return symbol.AllInterfaces.AddRange(types);

        //types.Reverse();

        types.AddRange(symbol.AllInterfaces);
        return types;
    }

    public static IEnumerable<IMethodSymbol> GetAllMethods(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetMembers().Where(member => member.Kind == SymbolKind.Method && member.DeclaredAccessibility == Accessibility.Public).Cast<IMethodSymbol>().Where(member => member.MethodKind == MethodKind.Ordinary));
    }

    public static IEnumerable<IPropertySymbol> GetAllProperties(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetMembers().Where(member => member.Kind == SymbolKind.Property && member.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>());
    }

    public static IEnumerable<AttributeData> GetAllAttributes(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetAttributes());
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypeMembers(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetTypeMembers());
    }
}