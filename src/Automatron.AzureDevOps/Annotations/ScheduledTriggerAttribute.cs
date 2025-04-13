using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class ScheduledTriggerAttribute(string cron) : Attribute
{
    public string Cron { get; } = cron;

    public string? DisplayName { get; set; }

    public bool Always { get; set; }

    public string[]? IncludeBranches { get; set; }

    public string[]? ExcludeBranches { get; set; }
}