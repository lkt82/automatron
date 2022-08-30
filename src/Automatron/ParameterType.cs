using System;
using System.Collections.Generic;

namespace Automatron;

#if NET6_0
internal record ParameterType(Type Type, IEnumerable<ParameterProperty> Parameters)
{
}

#endif