#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class PrTrigger: IPrTrigger
{
    public bool? AutoCancel { get; set; }

    public bool? Drafts { get; set; }

    public TriggerBranches? Branches { get; set; }

    public TriggerPaths? Paths { get; set; }
}
#endif