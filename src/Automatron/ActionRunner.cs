#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Automatron;

internal class ActionRunner : IActionRunner
{
    private readonly IServiceProvider _serviceProvider;

    public ActionRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static void Bind(object service, IEnumerable<ParameterDescriptor> parameters)
    {
        foreach (var parameterDescriptor in parameters)
        {
            parameterDescriptor.Property.SetValue(service, parameterDescriptor.Parameter.Value);
        }
    }

    public System.Threading.Tasks.Task Run(TaskContext context)
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService(context.Action.Type);

        foreach (var parameterTypeDescriptor in context.Parameters.Where(c=> c.Parameters.Any()))
        {
            var parameterTypeService = scope.ServiceProvider.GetRequiredService(parameterTypeDescriptor.Type);
            Bind(parameterTypeService, parameterTypeDescriptor.Parameters);
        }

        var result = context.Action.Invoke(service);

        if (result is System.Threading.Tasks.Task asyncResult)
        {
            return asyncResult;
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }
}
#endif