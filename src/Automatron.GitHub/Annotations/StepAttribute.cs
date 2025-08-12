using System;
using System.Collections.Generic;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class StepAttribute : NodeAttribute
{
    public string? WorkingDirectory { get; set; }

    public string[]? Needs { get; set; }

    public string? Uses { get; set; }

    public string? Run { get; set; }

    public string? Shell { get; set; }

    public Dictionary<string, string>? With { get; set; }

    public Dictionary<string, string>? Env { get; set; }

    public bool? ContinueOnError { get; set; }

    public int? TimeoutMinutes { get; set; }
}