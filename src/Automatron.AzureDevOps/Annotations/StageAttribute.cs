using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class StageAttribute : NodeAttribute
{
    public StageAttribute()
    {
    }

    public StageAttribute(string name)
    {
        Name = name;
    }

    public string[]? DependsOn { get; set; }

}