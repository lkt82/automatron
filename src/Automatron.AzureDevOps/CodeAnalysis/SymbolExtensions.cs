using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.CodeAnalysis;

public static class SymbolExtensions
{
    public static IEnumerable<T> GetCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.IsCustomAttribute<T>()).Select(c=> c.MapToCustomAttribute<T>());
    }

    public static T? GetCustomAttribute<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.IsCustomAttribute<T>()).Select(c => c.MapToCustomAttribute<T>()).FirstOrDefault();
    }

    public static bool HasCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Any(c => c.IsCustomAttribute<T>());
    }

    public static IEnumerable<T> GetCustomAbstractAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAbstractAttributes<T>();
    }

    public static T? GetCustomAbstractAttribute<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().GetCustomAbstractAttribute<T>();
    }
}