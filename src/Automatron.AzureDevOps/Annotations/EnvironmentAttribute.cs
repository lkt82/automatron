using System;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
public class EnvironmentAttribute : Attribute
{
    public EnvironmentAttribute(string environment)
    {
    }

    public EnvironmentAttribute()
    {
    }
}