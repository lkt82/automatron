using System;
using System.Collections.Generic;
using System.Reflection;

namespace Automatron;

#if NET6_0
public record ControllerTask(string Name,Type ControllerType, MethodInfo Action, IEnumerable<string> Dependencies)
{

}
#endif