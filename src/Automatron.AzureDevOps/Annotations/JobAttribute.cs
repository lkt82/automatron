using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class JobAttribute : Attribute
{
    public JobAttribute()
    {
    }

    public JobAttribute(string name)
    {
        Name = name;
    }

    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Condition { get; set; }

    public string? Emoji { get; set; }

    public string[]? DependsOn { get; set; }
}