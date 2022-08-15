#if NET6_0
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Automatron.Reflection;

public static class TypeExtension
{
    public static void Accept(this Type type, SymbolVisitor visitor)
    {
        visitor.VisitType(type);
    }

    public static IEnumerable<Type> AsFlat(this Type type)
    {
        var types = new List<Type>();

        var currentType = type;

        while (currentType != typeof(object))
        {
            types.Add(currentType);
          
            if (currentType.BaseType == null)
            {
                break;
            }

            currentType = currentType.BaseType;
        }

        types.Reverse();

        return type.GetInterfaces().ToImmutableArray().AddRange(types);
    }

    public static IEnumerable<MemberInfo> GetMethodAncestorsAndSelf(this Type type)
    {
        var baseType = type;

        while (baseType != null && baseType != typeof(object))
        {
            foreach (var method in baseType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                yield return method;
            }

            baseType = baseType.BaseType;
        }
    }
}
#endif