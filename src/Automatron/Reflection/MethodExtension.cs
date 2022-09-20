#if NET6_0
using System.Reflection;

namespace Automatron.Reflection;

public static class MethodExtension
{
    public static void Accept(this MethodInfo methodInfo, MemberVisitor visitor)
    {
        visitor.VisitMethod(methodInfo);
    }

    public static TResult? Accept<TResult>(this MethodInfo methodInfo, MemberVisitor<TResult> visitor)
    {
        return visitor.VisitMethod(methodInfo);
    }
}

#endif