#if NET6_0
using System;
using System.Reflection;

namespace Automatron.Models;

public class MethodAction : IAction
{
    public MethodInfo Method { get; }

    public MethodAction(MethodInfo method)
    {
        Method = method;
    }

    public MethodAction(string method,Type type) :this(type.GetMethod(method)!)
    {
    }

    public object? Invoke(object service)
    {
        return Method.Invoke(service, null);
    }
}

public class MethodAction<T> : MethodAction
{
    public MethodAction(string method) :base(method,typeof(T))
    {
    }
}

#endif