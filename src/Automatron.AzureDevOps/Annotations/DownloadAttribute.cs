using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class DownloadAttribute(string source) : NodeAttribute
{
    public string Source { get; } = source;
}