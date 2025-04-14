using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class PrTriggerAttribute : Attribute
{
    [UsedImplicitly]
    public PrTriggerAttribute()
    {

    }

    public bool AutoCancel { get; set; } = true;

    public bool Drafts { get; set; } = true;

    public bool Disabled { get; set; }

    public string[]? IncludeBranches { get; set; }

    public string[]? ExcludeBranches { get; set; }

    public string[]? IncludePaths { get; set; }

    public string[]? ExcludePaths { get; set; }
}