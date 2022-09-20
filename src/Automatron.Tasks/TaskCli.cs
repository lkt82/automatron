#if NET6_0
using Automatron.Tasks.Commands;
using Automatron.Tasks.Middleware;

namespace Automatron.Tasks;

public static class TaskCli
{
    public static AutomationRunner New()
    {
        return new AutomationRunner<TaskCommand>().UseTasks();
    }
}
#endif