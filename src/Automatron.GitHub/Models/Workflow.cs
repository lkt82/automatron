#if NET8_0

using System;
using System.Collections.Generic;

namespace Automatron.GitHub.Models;

public class Workflow
{
    public string Name { get; set; } = "";
    
    public string? DisplayName { get; set; }
    
    public Type Type { get; set; } = null!;
    
    public string YmlName { get; set; } = "";
    
    public string YmlDir { get; set; } = "./.github/workflows/";
    
    public string? RootDir { get; set; }
    
    public List<Job> Jobs { get; set; } = new();
    
    public List<Variable> Variables { get; set; } = new();
    
    //public List<Input> Inputs { get; set; } = new();
    
    public Dictionary<string, object> On { get; set; } = new();
    
    public Dictionary<string, object> Env { get; set; } = new();
    
    public Dictionary<string, object> Defaults { get; set; } = new();
    
    public Dictionary<string, object> Concurrency { get; set; } = new();
    
    public string[]? Permissions { get; set; }
}

#endif