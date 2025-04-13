#if NET8_0
using System.ComponentModel;

namespace Automatron.AzureDevOps.Models;

[TypeConverter(typeof(VariableValueConverter))]
public record VariableValue(string Name, string Value)
{
    public static VariableValue? Parse(string value)
    {
        var type = typeof(VariableValue);
        return (VariableValue)TypeDescriptor.GetConverter(type).ConvertTo(value, type)!;
    }
}

#endif