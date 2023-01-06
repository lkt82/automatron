#if NET6_0
using Automatron.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatron.AzureDevOps.Models;

public class Job : IEqualityComparer<Job>
{
    public Job(string name,Stage stage, IEnumerable<Step> steps, Type type)
    {
        Name = name;
        Stage = stage;
        Type = type;

        Steps = CreateSteps(steps.ToArray());
    }

    public Job(string name, Stage stage, Func<Job,IEnumerable<Step>> stepsFunc, Type type)
    {
        Name = name;

        Stage = stage;
        Type = type;

        Steps = CreateSteps(stepsFunc(this).ToArray());
    }

    private static IEnumerable<Step> CreateSteps(IEnumerable<Step> steps)
    {
        return new HashSet<Step>(steps.TopologicalSort(x => x.DependsOn));
    }

    public string Name { get; }

    public Stage Stage { get; }

    public Type Type { get; }

    public IEnumerable<Step> Steps { get; }

    public ISet<Job> DependsOn { get; } = new HashSet<Job>();

    public ISet<Variable> Variables { get; } = new HashSet<Variable>();

    public bool Equals(Job? x, Job? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name == y.Name && x.Stage.Equals(y.Stage);
    }

    public int GetHashCode(Job obj)
    {
        return HashCode.Combine(obj.Name, obj.Stage);
    }
}
#endif