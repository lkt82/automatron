using System;

namespace Automatron.Tasks.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ParameterAttribute : Attribute
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}