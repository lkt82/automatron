using System;
using System.ComponentModel;

namespace Automatron.Models;

[TypeConverter(typeof(SecretConverter))]
public sealed record Secret
{
    public static readonly string ValueReplacement = "*****";

    private readonly string _secret;

    public Secret(string value)
    {
        _secret = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string GetValue() => _secret;

    public override string ToString()
    {
        return string.IsNullOrEmpty(_secret) ? string.Empty : ValueReplacement;
    }
}