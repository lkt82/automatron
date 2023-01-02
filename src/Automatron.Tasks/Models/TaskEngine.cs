#if NET6_0
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Automatron.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Automatron.Tasks.Models;

internal class TaskEngine : ITaskEngine
{
    private readonly IServiceProvider _serviceProvider;

    public TaskEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static void BindProperties(object service, IEnumerable<IPropertyValue> variables)
    {
        foreach (var variable in variables)
        {
            if (variable.Value == null)
            {
                continue;
            }
            variable.Property.SetValue(service, variable.Value);
        }
    }

    private static void ConvertParameters(IEnumerable<ParameterValue> input, IDictionary<string, Parameter> variables)
    {
        foreach (var variable in input)
        {
            if (!variables.TryGetValue(variable.Name, out var foundVariable))
            {
                continue;
            }

            var conv = TypeDescriptor.GetConverter(foundVariable.Property.PropertyType);
            foundVariable.Value = conv.ConvertTo(variable.Value, foundVariable.Property.PropertyType);
        }
    }

    public System.Threading.Tasks.Task Run(Task task, IEnumerable<ParameterValue>? parameters)
    {
        ConvertParameters(parameters ?? Array.Empty<ParameterValue>(), task.ParameterTypes.SelectMany(c=> c.Parameters).ToDictionary(c => c.Name.ToLower(), c => c));

        using var scope = _serviceProvider.CreateAsyncScope();

        var service = scope.ServiceProvider.GetRequiredService(task.Type);

        foreach (var parameterType in task.ParameterTypes.Where(c=> c.Parameters.Any()))
        {
            var parameterTypeService = scope.ServiceProvider.GetRequiredService(parameterType.Type);
            BindProperties(parameterTypeService, parameterType.Parameters);
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