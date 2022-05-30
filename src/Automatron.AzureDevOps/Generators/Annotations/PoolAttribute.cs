using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
public class PoolAttribute : Attribute
{
    public string? Target { get; }

    public string? Name { get; set; }

    public string? VmImage { get; set; }

    [UsedImplicitly]
    public PoolAttribute()
    {
    }

    public PoolAttribute(string target)
    {
        Target = target;
    }
}