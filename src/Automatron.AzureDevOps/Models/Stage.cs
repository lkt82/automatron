#if NET6_0
using System;
using System.Collections.Generic;

namespace Automatron.AzureDevOps.Models;

public class Stage : IComparer<Job>, IEqualityComparer<Stage>
{
    public Stage(string name,Pipeline pipeline, IEnumerable<Job> jobs, Type type)
    {
        Name = name;
        Pipeline = pipeline;
        Type = type;
        Jobs = new SortedSet<Job>(jobs, this);
    }

    public Stage(string name, Pipeline pipeline, Func<Stage,IEnumerable<Job>> jobs, Type type)
    {
        Name = name;
        Pipeline = pipeline;
        Type = type;
        Jobs = new SortedSet<Job>(jobs(this), this);
    }

    public string Name { get; }

    public Pipeline Pipeline { get; }

    public Type Type { get; }

    public ISet<Job> Jobs { get; }

    public ISet<Stage> DependsOn { get; } = new HashSet<Stage>();

    public ISet<Variable> Variables { get; } = new HashSet<Variable>();

    public int Compare(Job? x, Job? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        if (x.DependsOn.Contains(y))
        {
            return 1;
        }

        if (y.DependsOn.Contains(x))
        {
            return -1;
        }

        return -1;
    }

    public bool Equals(Stage? x, Stage? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name == y.Name && x.Pipeline.Equals(y.Pipeline);
    }

    public int GetHashCode(Stage obj)
    {
        return HashCode.Combine(obj.Name, obj.Pipeline);
    }
}
#endif