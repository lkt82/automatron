using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class SecretVariableAttribute : Attribute
{
    public string? Name { get; set; }

    public SecretVariableAttribute()
    {
    }

}