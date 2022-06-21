using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ParameterAttribute : Attribute
{
    public string Pipeline { get; }
    public string Name { get; }
    public string? DisplayName { get; set; }
    public string Type { get; }
    public object? Value { get; set; }
    public string[]? Values { get; set; }

    public ParameterAttribute(string pipeline,string name,string type)
    {
        Pipeline = pipeline;
        Name = name;
        Type = type;
    }
}