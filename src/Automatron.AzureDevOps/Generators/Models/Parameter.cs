#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Parameter
{
    public Parameter(string name, string? displayName, string type, object? value, object[]? values)
    {
        Name = name;
        DisplayName = displayName;
        Type = type;
        Value = value;
        Values = values;
    }

    public string Name { get; }

    public string? DisplayName { get; set; }

    public string Type { get; }

    public object? Value { get; set; }

    public object[]? Values { get; set; }
}
#endif