#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Reflection;

namespace Automatron.AzureDevOps;

internal class AzureDevOpsTaskModelFactory: ITaskModelFactory
{
    private readonly IEnumerable<Type> _types;

    public AzureDevOpsTaskModelFactory(ITypeProvider typeProvider)
    {
        _types = typeProvider.Types;
    }

    public TaskModel Create()
    {
        var pipelineVisitor = new PipelineVisitor(_types);

        var types = _types.Where(c => !c.IsNested);

        foreach (var taskType in types)
        {
            taskType.Accept(pipelineVisitor);
        }

        return new TaskModel(pipelineVisitor.Tasks.Values, pipelineVisitor.Parameters.Values);
    }
}

#endif