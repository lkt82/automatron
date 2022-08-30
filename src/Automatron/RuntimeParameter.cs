using System;

namespace Automatron;

#if NET6_0
internal record RuntimeParameter(string Name, string? Description, Type Type) : Parameter(Name, Type, null)
{
}
#endif