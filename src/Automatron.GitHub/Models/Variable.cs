#if NET8_0
using System.Reflection;

namespace Automatron.GitHub.Models;

public class Variable
{
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    
    public object? Value { get; set; }
    
    public object? Default { get; set; }
    
    public PropertyInfo Property { get; set; } = null!;
}
#endif