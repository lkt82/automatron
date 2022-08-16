using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class PoolAttribute : Attribute
{
    public string? Name { get; set; }

    public string? VmImage { get; set; }
}