using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class KubeLoginInstallerAttribute : NodeAttribute
{
    public string? Version { get; set; }
}