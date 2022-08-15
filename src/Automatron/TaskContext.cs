#if NET6_0
using System.Collections.Generic;

namespace Automatron;

internal class TaskContext
{
    public TaskContext(ActionDescriptor actionDescriptor, IEnumerable<ParameterTypeDescriptor> parameterTypeDescriptors)
    {
        ActionDescriptor = actionDescriptor;
        ParameterTypeDescriptors = parameterTypeDescriptors;
    }

    public IEnumerable<ParameterTypeDescriptor> ParameterTypeDescriptors { get; set; }

    public ActionDescriptor ActionDescriptor { get; set; }
}
#endif