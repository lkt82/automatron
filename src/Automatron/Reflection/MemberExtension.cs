#if NET6_0
using System;
using System.Reflection;

namespace Automatron.Reflection;

public static class MemberExtension
{
    public static void Accept(this MemberInfo memberInfo, SymbolVisitor visitor)
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
}
#endif