#if NET6_0
using System;
using System.ComponentModel;
using System.Globalization;

namespace Automatron.AzureDevOps.Models;

public sealed class VariableValueConverter : TypeConverter
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value == null)
        {
            return null;
        }

        if (destinationType == typeof(VariableValue))
        {
            var keyValue = value.ToString()!.Split(":");
            var variableName = keyValue[0].ToLower();
            var variableValue = keyValue[1];
            return new VariableValue(variableName, variableValue);
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }
}

#endif