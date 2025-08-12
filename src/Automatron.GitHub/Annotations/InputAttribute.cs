using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class InputAttribute : Attribute
{
    public string? Description { get; set; }

    public bool Required { get; set; }

    public object? Default { get; set; }

    public string? Type { get; set; }

    public string[]? Options { get; set; }
}