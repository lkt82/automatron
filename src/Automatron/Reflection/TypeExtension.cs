#if NET6_0
using System;

namespace Automatron.Reflection;

public static class TypeExtension
{
    public static void Accept(this Type type, SymbolVisitor visitor)
    {
        visitor.VisitType(type);
    }

}
#endif