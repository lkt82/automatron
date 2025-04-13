#if NET8_0
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Automatron.Reflection;

public static class MemberExtension
{
    public static void Accept(this MemberInfo memberInfo, MemberVisitor visitor)
    {
        switch (memberInfo)
        {
            case Type type:
                type.Accept(visitor);
                break;
            case ConstructorInfo constructor:
                constructor.Accept(visitor);
                break;
            case MethodInfo method:
                method.Accept(visitor);
                break;
            case PropertyInfo property:
                property.Accept(visitor);
                break;
        }
    }


    public static IEnumerable<T> GetAllCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
    {
        if (memberInfo is Type type)
        {
            return type.GetAllCustomAttributes<T>();
        }


        if (memberInfo is MethodInfo methodInfo)
        {
            return methodInfo.GetAllCustomAttributes<T>();
        }

        return memberInfo.GetCustomAttributes<T>();
    }

    public static T? GetAllCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
    {
        if (memberInfo is Type type)
        {
            return type.GetAllCustomAttribute<T>();
        }

        return memberInfo.GetCustomAttribute<T>();
    }
}
#endif