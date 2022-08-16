#if NET6_0
using System.Reflection;

namespace Automatron.Reflection;

public static class ConstructorExtension
{
    public static void Accept(this ConstructorInfo constructorInfo, SymbolVisitor visitor)
    {
        visitor.VisitConstructor(constructorInfo);
    }
}
#endif