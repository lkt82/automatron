using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class ParameterAttribute : Attribute
{
    public string? Pipeline { get; }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public string Type { get; }

    public object? Value { get; set; }

    public object[]? Values { get; set; }

    public ParameterAttribute(string pipeline,string name,string type)
    {
        Pipeline = pipeline;
        Name = name;
        Type = type;
    }

    public ParameterAttribute(string name, string type)
    {
        Name = name;
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class BooleanParameterAttribute : ParameterAttribute
{
    public new bool Value { get; set; }

    public new bool[]? Values { get; set; }

    public BooleanParameterAttribute(string pipeline, string name) :base(pipeline, name, ParameterTypes.Boolean)
    {

    }
}