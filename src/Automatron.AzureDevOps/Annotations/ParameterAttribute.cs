using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public string? Type { get; set; }

    public object? Default { get; set; }

    public object[]? Values { get; set; }
}