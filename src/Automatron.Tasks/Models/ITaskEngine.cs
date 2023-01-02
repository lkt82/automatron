#if NET6_0

using System.Collections.Generic;

namespace Automatron.Tasks.Models;

public interface ITaskEngine
{
    public System.Threading.Tasks.Task Run(Task task, IEnumerable<ParameterValue>? parameters);
}
#endif