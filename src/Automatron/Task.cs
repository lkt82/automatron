#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class Task
{
    public Task(string name, ISet<Task> dependencies, Action action, IEnumerable<ParameterType> parameters)
    {
        Action = action;
        Parameters = parameters;
        Name = name;
        Dependencies = dependencies;
    }

    public string Name { get; }

    public bool Default { get; set; }

    public ISet<Task> Dependencies { get; }

    public Action Action { get; }

    public IEnumerable<ParameterType> Parameters { get; set; }
}
#endif