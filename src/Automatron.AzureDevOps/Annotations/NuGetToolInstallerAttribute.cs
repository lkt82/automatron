using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class NuGetToolInstallerAttribute : NodeAttribute
{
    public string? VersionSpec { get; set; }

    public bool CheckLatest { get; set; }
}