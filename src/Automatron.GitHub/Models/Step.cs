#if NET8_0

using System.Collections.Generic;
using System.Reflection;
using Automatron.Models;

namespace Automatron.GitHub.Models;

public class Step
{
    public string Name { get; set; } = "";
    
    public string? DisplayName { get; set; }
    
    public MethodInfo Method { get; set; } = null!;
    
    public string? If { get; set; }
    
    public string[]? Needs { get; set; }
    
    public string? Uses { get; set; }
    
    public string? Run { get; set; }
    
    public string? Shell { get; set; }
    
    public string? WorkingDirectory { get; set; }
    
    public Dictionary<string, object>? With { get; set; }
    
    public Dictionary<string, object>? Env { get; set; }
    
    public bool? ContinueOnError { get; set; }
    
    public int? TimeoutMinutes { get; set; }
    
    public IAction Action { get; set; } = null!;
}
#endif