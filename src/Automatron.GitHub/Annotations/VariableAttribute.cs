using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class VariableAttribute : Attribute
{
    public string? Description { get; set; }

    public object? Default { get; set; }
}