using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class WorkflowAttribute : Attribute
{
    public string? Name { get; }

    public string? YmlName { get; set; }

    public string YmlDir { get; set; } = "./.github/workflows/";

    public string? RootDir { get; set; }

    public WorkflowAttribute(string name)
    {
        Name = name;
    }

    public WorkflowAttribute()
    {
    }
}