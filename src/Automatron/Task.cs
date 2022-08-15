#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class Task
{
    public Task(string name, ISet<Task> dependencies, ActionDescriptor actionDescriptor, IEnumerable<ParameterTypeDescriptor> parameterDescriptors)
    {
        ActionDescriptor = actionDescriptor;
        ParameterDescriptors = parameterDescriptors;
        Name = name;
        Dependencies = dependencies;
    }

    public string Name { get; }

    public bool Default { get; set; }

    public ISet<Task> Dependencies { get; }

    public ActionDescriptor ActionDescriptor { get; }

    public IEnumerable<ParameterTypeDescriptor> ParameterDescriptors { get; set; }
}
#endif