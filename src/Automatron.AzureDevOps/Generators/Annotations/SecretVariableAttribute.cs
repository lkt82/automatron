using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class SecretVariableAttribute : Attribute
{
    public string? Pipeline { get; }

    public string? Name { get; set; }

    public SecretVariableAttribute()
    {
    }

    public SecretVariableAttribute(string pipeline)
    {
        Pipeline = pipeline;
    }
}