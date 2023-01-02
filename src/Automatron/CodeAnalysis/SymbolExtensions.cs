using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Automatron.CodeAnalysis;

public static class SymbolExtensions
{
    public static IEnumerable<T> GetCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAttributes<T>();
    }

    public static T? GetCustomAttribute<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAttribute<T>();
    }
}