#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
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

    private static Dictionary<MemberInfo, List<Attribute>> Cache { get; } = new();

    public static T? GetCachedAttribute<T>(this MemberInfo member, bool inherit = true) where T : Attribute
    {
        List<Attribute> list;

        if (!Cache.ContainsKey(member))
        {
            list = new List<Attribute>();
            Cache.Add(member, list);
        }

        list = Cache[member];

        var attribute2 = list.OfType<T>().FirstOrDefault();

        if (attribute2 != null)
        {
            return attribute2;
        }

    
        var attributes = member.GetCustomAttributes<T>(inherit);
        foreach (var attribute in attributes)
        {
            list.Add(attribute);
        }

        return list.OfType<T>().FirstOrDefault();
    }
}
#endif