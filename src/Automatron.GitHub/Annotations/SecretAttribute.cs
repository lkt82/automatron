using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class SecretAttribute : Attribute
{
    public string? Description { get; set; }
}