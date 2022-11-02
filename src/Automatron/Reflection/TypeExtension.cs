#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automatron.Reflection;

public static class TypeExtension
{
    public static void Accept(this Type type, MemberVisitor visitor)
    {
        visitor.VisitType(type);
    }

    public static TResult? Accept<TResult>(this Type type, MemberVisitor<TResult> visitor)
    {
        return visitor.VisitType(type);
    }

    public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
    {
        return type.GetHierarchy().SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    public static IEnumerable<Type> GetAllNestedTypes(this Type type)
    {
        return type.GetHierarchy().SelectMany(c => c.GetNestedTypes());
    }


    public static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
    {
        return type.GetHierarchy().SelectMany(c => c.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }

    public static IEnumerable<T> GetAllCustomAttributes<T>(this Type type) where T : Attribute
    {
        return type.GetHierarchy().SelectMany(c => c.GetCustomAttributes<T>(false));
    }

    public static T? GetAllCustomAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetHierarchy().SelectMany(c => c.GetCustomAttributes<T>(false)).FirstOrDefault();
    }

    public static IEnumerable<Type> GetHierarchy(this Type type)
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

        types.AddRange(type.GetInterfaces());
        return types;
    }

}
#endif