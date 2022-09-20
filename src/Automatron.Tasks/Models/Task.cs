#if NET6_0
using System;
using System.Collections.Generic;
using Automatron.Models;

namespace Automatron.Tasks.Models;

public class Task
{
    public Task(string name, ISet<Task> dependencies, IAction action, IEnumerable<ParameterType> parameters,Type type)
    {
        Action = action;
        Parameters = parameters;
        Type = type;
        Name = name;
        Dependencies = dependencies;
    }

    public string Name { get; }

    public bool Default { get; set; }

    public ISet<Task> Dependencies { get; }

    public IAction Action { get; }

    public IEnumerable<ParameterType> Parameters { get; set; }

    public Type Type { get; }
}
#endif