#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Automatron.Tasks.Models;

internal class TaskRunner : ITaskRunner
{
    private readonly IServiceProvider _serviceProvider;

    public TaskRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static void Bind(object service, IEnumerable<Parameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            parameter.Property.SetValue(service, parameter.Value);
        }
    }

    public System.Threading.Tasks.Task Run(Task task)
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService(task.Type);

        foreach (var parameterType in task.Parameters.Where(c=> c.Parameters.Any()))
        {
            var parameterTypeService = scope.ServiceProvider.GetRequiredService(parameterType.Type);
            Bind(parameterTypeService, parameterType.Parameters);
        }

        var result = task.Action.Invoke(service);

        if (result is System.Threading.Tasks.Task asyncResult)
        {
            return asyncResult;
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }
}
#endif