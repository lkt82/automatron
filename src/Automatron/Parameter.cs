#if NET6_0
using System;

namespace Automatron;

internal abstract record Parameter(string Name, Type Type, object? Value)
{
    public object? Value { get; set; } = Value;
}

#endif