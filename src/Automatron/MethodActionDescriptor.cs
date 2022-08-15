using System;
using System.Reflection;

namespace Automatron;

#if NET6_0
internal record MethodActionDescriptor(MethodInfo Method, Type Type) : ActionDescriptor(Type)
{
    public override object? Invoke(object service)
    {
        return Method.Invoke(service, null);
    }
}
#endif