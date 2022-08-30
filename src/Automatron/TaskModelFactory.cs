#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Reflection;

namespace Automatron;

internal class TaskModelFactory : ITaskModelFactory
{
    private readonly Func<TaskVisitor> _visitorFactory;
    private readonly IEnumerable<Type> _types;

    public TaskModelFactory(ITypeProvider typeProvider,Func<TaskVisitor> visitorFactory)
    {
        _visitorFactory = visitorFactory;
        _types = typeProvider.Types;
    }

    public TaskModel Create()
    {
        var taskVisitor = _visitorFactory();

        var types = _types.Where(c => !c.IsNested);

        foreach (var taskType in types)
        {
            taskType.Accept(taskVisitor);
        }

        return new TaskModel(taskVisitor.Tasks.Values, taskVisitor.Parameters.Values.OfType<RuntimeParameter>());
    }
}
#endif