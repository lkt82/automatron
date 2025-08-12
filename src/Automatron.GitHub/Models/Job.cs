#if NET8_0
using System;
using System.Collections.Generic;

namespace Automatron.GitHub.Models;

public class Job
{
    public string Name { get; set; } = "";
    
    public string? DisplayName { get; set; }
    
    public Type Type { get; set; } = null!;
    
    public string? If { get; set; }
    
    public string[]? Needs { get; set; }
    
    public string? RunsOn { get; set; }
    
    public Dictionary<string, object>? Strategy { get; set; }
    
    public int? TimeoutMinutes { get; set; }
    
    public bool? CancelInProgress { get; set; }
    
    public List<Step> Steps { get; set; } = new();
    
    public List<Variable> Variables { get; set; } = new();
    
    public Dictionary<string, object> Env { get; set; } = new();
    
    public Dictionary<string, object> Defaults { get; set; } = new();
    
    public object? Container { get; set; }
    
    public object[]? Services { get; set; }
}
#endif