using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class PipelineTriggerAttribute : Attribute
{
    public PipelineTriggerAttribute(string name,string source)
    {
        Name = name;
        Source = source;
    }

    public string Name { get; }

    public string Source { get; set; }

    public string? Project { get; set; }

    public string[]? Stages { get; set; }

    public string[]? Tags { get; set; }

    public string[]? IncludeBranches { get; set; }

    public string[]? ExcludeBranches { get; set; }
}