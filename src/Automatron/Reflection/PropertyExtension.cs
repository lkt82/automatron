#if NET6_0
using System.Reflection;

namespace Automatron.Reflection;

public static class PropertyExtension
{
    public static void Accept(this PropertyInfo propertyInfo, SymbolVisitor visitor)
    {
        visitor.VisitProperty(propertyInfo);
    }
}
#endif