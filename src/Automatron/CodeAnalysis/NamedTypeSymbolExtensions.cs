#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.CodeAnalysis;

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

        types.AddRange(symbol.AllInterfaces);

        types.Reverse();

        return types;
    }

    public static IEnumerable<IMethodSymbol> GetAllMethods(this INamedTypeSymbol symbol)
    {
        var allMethods = symbol.GetHierarchy().SelectMany(c => c.GetMembers().Where(member => member is { Kind: SymbolKind.Method, DeclaredAccessibility: Accessibility.Public }).Cast<IMethodSymbol>().Where(member => member.MethodKind == MethodKind.Ordinary)).ToList();

        var array = allMethods.ToArray();

        foreach (var methodSymbol in array)
        {
            if (methodSymbol.IsOverride)
            {
                var symbol1 = methodSymbol;
                var index = allMethods.FindIndex(c => c.Name == symbol1.Name);
                allMethods.RemoveAt(index);
            }
        }

        return allMethods;
    }

    public static IEnumerable<IPropertySymbol> GetAllProperties(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetMembers().Where(member => member is { Kind: SymbolKind.Property, DeclaredAccessibility: Accessibility.Public }).Cast<IPropertySymbol>());
    }

    public static IEnumerable<AttributeData> GetAllAttributes(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetAttributes());
    }

    public static IEnumerable<INamedTypeSymbol> GetAllTypeMembers(this INamedTypeSymbol symbol)
    {
        return symbol.GetHierarchy().SelectMany(c => c.GetTypeMembers());
    }

    public static IEnumerable<T> GetAllCustomAttributes<T>(this INamedTypeSymbol symbol) where T : Attribute
    {
        return symbol.GetAllAttributes().GetCustomAttributes<T>();
    }
}
#endif