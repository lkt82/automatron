#if NET6_0
using Automatron.Tasks.Commands;
using Automatron.Tasks.Middleware;

namespace Automatron.Tasks;

public class TaskRunner : AutomationRunner<TaskCommand>
{
    public TaskRunner()
    {
        this.UseTasks();
    }

}
#endif