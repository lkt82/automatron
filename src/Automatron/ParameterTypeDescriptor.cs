using System;
using System.Collections.Generic;

namespace Automatron;

#if NET6_0
internal record ParameterTypeDescriptor(Type Type, IEnumerable<ParameterDescriptor> Parameters)
{
}

#endif