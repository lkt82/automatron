#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class TaskContext
{
    public TaskContext(Action action, IEnumerable<ParameterType> parameters)
    {
        Action = action;
        Parameters = parameters;
    }

    public IEnumerable<ParameterType> Parameters { get; set; }

    public Action Action { get; set; }
}
#endif