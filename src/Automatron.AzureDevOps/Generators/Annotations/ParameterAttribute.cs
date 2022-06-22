using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
    public string? Pipeline { get; }

    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Type { get; set; }

    public object? Default { get; set; }

    public object[]? Values { get; set; }

    public ParameterAttribute(string pipeline)
    {
        Pipeline = pipeline;
    }

    public ParameterAttribute()
    {
    }
}