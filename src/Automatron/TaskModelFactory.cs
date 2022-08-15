#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Reflection;

namespace Automatron;

internal class TaskModelFactory : ITaskModelFactory
{
    private readonly IEnumerable<Type> _types;

    public TaskModelFactory(ITypeProvider typeProvider)
    {
        _types = typeProvider.GetTypes();
    }

    public TaskModel Create()
    {
        var taskVisitor = new TaskVisitor(_types);

        foreach (var taskType in _types.Where(c=> !c.IsNested))
        {
            taskType.Accept(taskVisitor);
        }

        return new TaskModel(taskVisitor.Tasks.Values, taskVisitor.Parameters.Values);
    }
}
#endif