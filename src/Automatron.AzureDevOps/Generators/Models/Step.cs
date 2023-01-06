using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public abstract class Step: IEqualityComparer<Step>
{
    [YamlIgnore]
    public IJob Job { get; }

    protected Step(IJob job)
    {
        Job = job;
        Id = Guid.NewGuid().ToString("n");
    }

    [YamlIgnore]
    public string Id { get; set; }

    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    [YamlIgnore] public ISet<Step> DependsOn { get; set; } = new HashSet<Step>();

    public string? Condition { get; set; }

    public bool Equals(Step? x, Step? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Id == y.Id && x.Job.Equals(y.Job);
    }

    public int GetHashCode(Step obj)
    {
        return obj.Id.GetHashCode();
    }
}