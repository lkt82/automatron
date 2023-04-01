#if NETSTANDARD2_0
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;

namespace Automatron.CodeAnalysis;

public static class MethodSymbolExtensions
{
    public static IEnumerable<T> GetAllCustomAttributes<T>(this IMethodSymbol symbol) where T : Attribute
    {
        var list = new List<T>();

        var current = symbol;

        list.AddRange(current.GetCustomAttributes<T>());

        while (current.IsOverride)
        {
            current = symbol.OverriddenMethod;

            if (current == null)
            {
                break;
            }

            list.AddRange(current.GetCustomAttributes<T>());
            
        }

        list.Reverse();
        return list;
    }
}
#endif