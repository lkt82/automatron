using System;
using System.Collections.Generic;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class WorkflowDispatchAttribute : Attribute
{
    public Dictionary<string, object>? Inputs { get; set; }
}