#if NET6_0
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automatron.Reflection;

public static class AttributeExtension
{
    public static IEnumerable<T> GetCachedCustomAttributes<T>(this MemberInfo member)
        where T : Attribute => Cache
        .GetOrAdd(member, t => t.GetCustomAttributes(true).OfType<Attribute>().ToArray())
        .OfType<T>();

    public static T? GetCachedCustomAttribute<T>(this MemberInfo member)
        where T : Attribute => member.GetCachedCustomAttributes<T>().FirstOrDefault();

    private static ConcurrentDictionary<MemberInfo, Attribute[]> Cache { get; } = new();

}
#endif