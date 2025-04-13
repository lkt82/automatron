#if NET8_0
using System;
using System.Collections.Generic;
using Automatron.Models;

namespace Automatron.AzureDevOps.Models;

public class Step(string name, Job job, IAction action) : IEqualityComparer<Step>
{
    public Step(string name, Job job, string method) : this(name, job, new MethodAction(method, job.Type))
    {
    }

    public Step(Job job, string method) : this(method, job, new MethodAction(method, job.Type))
    {
    }

    public string Name { get; } = name;

    public Job Job { get; } = job;

    public IAction Action { get; } = action;

    public ISet<Step> DependsOn { get; } = new HashSet<Step>();

    public bool Equals(Step? x, Step? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name == y.Name && x.Job.Equals(y.Job);
    }

    public int GetHashCode(Step obj)
    {
        return HashCode.Combine(obj.Name, obj.Job);
    }
}
#endif