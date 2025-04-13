using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface)]
public class ParameterAttribute : Attribute
{
    public ParameterAttribute(string name)
    {
        Name = name;
    }

    public ParameterAttribute()
    {
    }

    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public object? Value { get; set; }

    public object[]? Values { get; set; }
}