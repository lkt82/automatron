using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class RunsOnAttribute(string runner) : Attribute
{
    public string Runner { get; } = runner;

    public string[]? Labels { get; set; }
}