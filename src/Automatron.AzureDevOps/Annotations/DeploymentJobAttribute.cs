using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class DeploymentJobAttribute : JobAttribute
{
    public string? Environment { get; set; }

    public string? Timeout { get; set; }

    public DeploymentJobAttribute()
    {
    }

    public DeploymentJobAttribute(string name) :base(name)
    {
    }
}