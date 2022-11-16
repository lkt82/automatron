using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class DownloadAttribute: NodeAttribute
{
    public string Source { get; }

    public DownloadAttribute(string source)
    {
        Source = source;
    }
}