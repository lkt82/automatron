#if NET8_0
using System.Reflection;
using Automatron.Models;

namespace Automatron.AzureDevOps.Models;

public record Parameter(string Name, string? DisplayName, PropertyInfo Property) : IPropertyValue
{
    public object? Value { get; set; }
}
#endif