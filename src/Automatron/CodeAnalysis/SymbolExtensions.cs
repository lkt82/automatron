#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Automatron.CodeAnalysis;

public static class SymbolExtensions
{
    public static IEnumerable<T> GetAllCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        if (symbol is INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.GetAllCustomAttributes<T>();
        }

        if (symbol is IMethodSymbol methodSymbol)
        {
            return methodSymbol.GetAllCustomAttributes<T>();
        }

        return [];
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAttributes<T>();
    }

    public static T? GetCustomAttribute<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAttribute<T>();
    }
}
#endif