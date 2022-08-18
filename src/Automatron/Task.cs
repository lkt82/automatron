#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class Task
{
    public Task(string name, ISet<Task> dependencies, ActionDescriptor action, IEnumerable<ParameterTypeDescriptor> parameters)
    {
        Action = action;
        Parameters = parameters;
        Name = name;
        Dependencies = dependencies;
    }

    public string Name { get; }

    public bool Default { get; set; }

    public ISet<Task> Dependencies { get; }

    public ActionDescriptor Action { get; }

    public IEnumerable<ParameterTypeDescriptor> Parameters { get; set; }
}
#endif