using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

public static class SymbolExtensions
{
    public static IEnumerable<T> GetCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.IsCustomAttribute<T>()).Select(c=> c.MapToCustomAttribute<T>());
    }

    public static bool HasCustomAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Any(c => c.IsCustomAttribute<T>());
    }

    public static IEnumerable<T> GetCustomAbstractAttributes<T>(this ISymbol symbol) where T : Attribute
    {
        return symbol.GetAttributes().Where(c => c.AttributeClass!.BaseType!.Name == typeof(T).Name).Select(c=> c.MapToCustomAttribute<T>(Type.GetType(c.AttributeClass + ", " + c.AttributeClass?.ContainingAssembly) ?? throw new InvalidOperationException()));
    }

}