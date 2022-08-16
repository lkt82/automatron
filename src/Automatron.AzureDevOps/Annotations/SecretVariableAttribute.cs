using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class SecretVariableAttribute : Attribute
{
    public string? Name { get; set; }

    public SecretVariableAttribute()
    {
    }

}