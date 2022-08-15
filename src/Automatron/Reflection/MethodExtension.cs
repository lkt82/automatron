#if NET6_0
using System.Reflection;

namespace Automatron.Reflection;

public static class MethodExtension
{
    public static void Accept(this MethodInfo methodInfo, SymbolVisitor visitor)
    {
        visitor.VisitMethod(methodInfo);
    }
}

#endif