using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
public class EnvironmentAttribute : TemplateParameterAttribute
{
    public EnvironmentAttribute(string environment) :base("Environment")
    {
        Value = environment;
    }

    public EnvironmentAttribute()
    {
    }
}