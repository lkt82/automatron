using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface)]
public class TemplateParameterAttribute : Attribute
{
    public TemplateParameterAttribute(string name)
    {
        Name = name;
    }

    public TemplateParameterAttribute()
    {
    }

    public string? Name { get; set; }

    public object? Value { get; set; }
}