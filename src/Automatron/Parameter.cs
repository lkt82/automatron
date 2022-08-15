#if NET6_0
using System;

namespace Automatron;

internal record Parameter(string Name, string? Description,Type Type)
{
    public object? Value { get; set; }
}
#endif