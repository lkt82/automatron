#if NETSTANDARD2_0
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Automatron.CodeAnalysis;

public static class MethodSymbolExtensions
{
    public static IEnumerable<T> GetAllCustomAttributes<T>(this IMethodSymbol symbol) where T : Attribute
    {
        var list = new List<IEnumerable<T>>();

        var current = symbol;

        list.Add(current.GetCustomAttributes<T>());

        while (current.IsOverride)
        {
            current = symbol.OverriddenMethod;

            if (current == null)
            {
                break;
            }

            list.Add(current.GetCustomAttributes<T>());

        }

        list.Reverse();

        return list.SelectMany(c => c);
    }
}
#endif