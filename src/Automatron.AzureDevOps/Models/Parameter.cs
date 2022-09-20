using System.Reflection;
using Automatron.Models;

namespace Automatron.AzureDevOps.Models;

#if NET6_0
public record Parameter(string Name, string? Description, PropertyInfo Property) : IPropertyValue
{
    public object? Value { get; set; }
}
#endif