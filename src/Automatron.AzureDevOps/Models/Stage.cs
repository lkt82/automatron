#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.Collections;

namespace Automatron.AzureDevOps.Models;

public class Stage : IEqualityComparer<Stage>
{
    public Stage(string name,Pipeline pipeline, IEnumerable<Job> jobs, Type type)
    {
        Name = name;
        Pipeline = pipeline;
        Type = type;
        Jobs = CreateJobs(jobs.ToArray());
    }

    public Stage(string name, Pipeline pipeline, Func<Stage,IEnumerable<Job>> jobs, Type type)
    {
        Name = name;
        Pipeline = pipeline;
        Type = type;
        Jobs = CreateJobs(jobs(this).ToArray());
    }

    private static IEnumerable<Job> CreateJobs(IEnumerable<Job> jobs)
    {
        return new HashSet<Job>(jobs.TopologicalSort(x => x.DependsOn));
    }

    public string Name { get; }

    public Pipeline Pipeline { get; }

    public Type Type { get; }

    public IEnumerable<Job> Jobs { get; }

    public ISet<Stage> DependsOn { get; } = new HashSet<Stage>();

    public ISet<Variable> Variables { get; } = new HashSet<Variable>();

    public IDictionary<string, object> TemplateValues { get; } = new Dictionary<string, object>();

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