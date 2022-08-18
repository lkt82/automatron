#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class TaskContext
{
    public TaskContext(ActionDescriptor action, IEnumerable<ParameterTypeDescriptor> parameters)
    {
        Action = action;
        Parameters = parameters;
    }

    public IEnumerable<ParameterTypeDescriptor> Parameters { get; set; }

    public ActionDescriptor Action { get; set; }
}
#endif