#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Reflection;

namespace Automatron.AzureDevOps;

internal class AzureDevOpsTaskModelFactoryDecorator: ITaskModelFactory
{
    private readonly ITaskModelFactory _taskModelFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITypeProvider _typeProvider;

    public AzureDevOpsTaskModelFactoryDecorator(
        ITaskModelFactory taskModelFactory,
        IServiceProvider serviceProvider,
        ITypeProvider typeProvider)
    {
        _taskModelFactory = taskModelFactory;
        _serviceProvider = serviceProvider;
        _typeProvider = typeProvider;
    }

    public IEnumerable<ITask> Create()
    {
        var tasks = _taskModelFactory.Create();

        var pipelineVisitor = new PipelineVisitor(_serviceProvider);

        foreach (var type in _typeProvider.GetTypes())
        {
            type.Accept(pipelineVisitor);
        }

        return tasks.Concat(pipelineVisitor.Tasks.Values);
    }
}

#endif