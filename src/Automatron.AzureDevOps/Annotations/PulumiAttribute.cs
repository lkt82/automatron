using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class PulumiAttribute : NodeAttribute
{
    public string? Command { get; set; }

    public string? Stack { get; set; }

    public string? Cwd { get; set; }

    public string? Args { get; set; }
}