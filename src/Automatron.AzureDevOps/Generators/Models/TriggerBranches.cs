#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class TriggerBranches
{
    public string[]? Include { get; set; }

    public string[]? Exclude { get; set; }
}

#endif