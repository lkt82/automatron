#if NET8_0
using System;
using System.Reflection;
using System.Threading;

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

    public object? Invoke(object service, CancellationToken cancellationToken)
    {
        var parameters = Method.GetParameters();
        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(CancellationToken))
        {
            return Method.Invoke(service, [cancellationToken]);
        }

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