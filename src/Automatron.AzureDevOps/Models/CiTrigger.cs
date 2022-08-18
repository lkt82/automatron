namespace Automatron.AzureDevOps.Models;

public sealed class CiTrigger: ICiTrigger
{
    public bool? Batch { get; set; }

    public TriggerBranches? Branches { get; set; }

    public TriggerPaths? Paths { get; set; }
}