using System;

namespace Automatron;

#if NET6_0
internal record ComputedParameter(string Name, Type Type, object? Value) : Parameter(Name, Type, Value)
{
}
#endif