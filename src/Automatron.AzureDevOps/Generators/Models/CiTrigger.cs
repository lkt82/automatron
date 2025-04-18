﻿#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class CiTrigger: ICiTrigger
{
    public bool? Batch { get; set; }

    public TriggerBranches? Branches { get; set; }

    public TriggerPaths? Paths { get; set; }
}
#endif