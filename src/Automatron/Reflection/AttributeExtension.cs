#if NET6_0
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Automatron.Reflection;

public static class AttributeExtension
{
    public static void Accept(this Attribute attribute, SymbolVisitor visitor)
    {
        visitor.VisitAttribute(attribute);
    }

    public static void Accept(this CustomAttributeData attributeData, SymbolVisitor visitor)
    {
        visitor.VisitAttributeData(attributeData);
    }

    private static Dictionary<MemberInfo, Attribute> Cache { get; } = new();

    public static T? GetCachedAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
    {
        if (Cache.TryGetValue(member, out var attribute))
        {
            return (T)attribute;
        }

        var customAttribute = member.GetCustomAttribute<T>(inherit);
        if (customAttribute != null)
        {
            Cache.Add(member, customAttribute);
        }

        return customAttribute;
    }
}
#endif