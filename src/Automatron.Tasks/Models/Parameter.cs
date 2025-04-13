#if NET8_0
using Automatron.Models;
using System.Reflection;

namespace Automatron.Tasks.Models;

public record Parameter(string Name, string? Description, PropertyInfo Property): IPropertyValue
{
    public object? Value { get; set; }
}

#endif