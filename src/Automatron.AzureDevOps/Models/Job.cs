#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatron.AzureDevOps.Models;

public class Job : IComparer<Step>, IEqualityComparer<Job>
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

    private ISet<Step> CreateSteps(IReadOnlyList<Step> steps)
    {
        for (var i = 1; i < steps.Count; i++)
        {
            if (steps[i].DependsOn.Any())
            {
                continue;
            }

            steps[i].DependsOn.Add(steps[i - 1]);
        }

        return new SortedSet<Step>(steps, this);
    }

    public string Name { get; }

    public Stage Stage { get; }

    public Type Type { get; }

    public ISet<Step> Steps { get; }

    public ISet<Job> DependsOn { get; } = new HashSet<Job>();

    public ISet<Variable> Variables { get; } = new HashSet<Variable>();

    public int Compare(Step? x, Step? y)
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