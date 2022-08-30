using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
public class EnvironmentAttribute : ParameterAttribute
{
    public EnvironmentAttribute(string environment)
    {
        Name = "Environment";
        Value = environment;
    }

    public EnvironmentAttribute()
    {
    }
}