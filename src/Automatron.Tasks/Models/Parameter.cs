#if NET6_0
using System.Reflection;

namespace Automatron.Tasks.Models;

public record Parameter(string Name, string? Description, PropertyInfo Property)
{
    public object? Value { get; set; }
}

#endif