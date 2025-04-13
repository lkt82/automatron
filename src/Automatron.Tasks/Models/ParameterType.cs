using System;
using System.Collections.Generic;

namespace Automatron.Tasks.Models;

#if NET8_0
public record ParameterType(Type Type, IEnumerable<Parameter> Parameters)
{
}

#endif