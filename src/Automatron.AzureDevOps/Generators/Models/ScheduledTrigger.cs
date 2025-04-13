#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class ScheduledTrigger(string cron)
{
    public string Cron { get; set; } = cron;

    public string? DisplayName { get; set; }

    public bool? Always { get; set; }

    public TriggerBranches? Branches { get; set; }
}

#endif