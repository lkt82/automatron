#if NET8_0
using System;
using System.ComponentModel;
using System.Globalization;

namespace Automatron.Models;

internal sealed class SecretConverter : TypeConverter
{
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value == null)
        {
            return null;
        }

        return destinationType == typeof(Secret) ? new Secret(value.ToString() ?? throw new InvalidOperationException()) : base.ConvertTo(context, culture, value, destinationType);
    }
}
#endif