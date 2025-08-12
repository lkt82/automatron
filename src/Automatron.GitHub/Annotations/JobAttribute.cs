using System;
using System.Collections.Generic;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class JobAttribute : NodeAttribute
{
    public string[]? Needs { get; set; }

    public string? Strategy { get; set; }

    public Dictionary<string, object>? Matrix { get; set; }

    public int? TimeoutMinutes { get; set; }

    public bool? CancelInProgress { get; set; }
}