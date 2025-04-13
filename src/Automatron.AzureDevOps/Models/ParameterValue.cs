using System.ComponentModel;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Models;

#if NET8_0
[TypeConverter(typeof(ParameterValueConverter))]
public record ParameterValue(string Name, string Value)
{
    [UsedImplicitly]
    public static ParameterValue? Parse(string value)
    {
        var type = typeof(ParameterValue);
        return (ParameterValue)TypeDescriptor.GetConverter(type).ConvertTo(value, type)!;
    }
}
#endif