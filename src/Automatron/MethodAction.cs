using System;
using System.Reflection;

namespace Automatron;

#if NET6_0
internal record MethodAction(MethodInfo Method, Type Type) : Action(Type)
{
    public override object? Invoke(object service)
    {
        return Method.Invoke(service, null);
    }
}
#endif