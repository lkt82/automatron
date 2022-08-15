#if NET6_0

namespace Automatron;

internal interface IActionRunner
{
    public System.Threading.Tasks.Task Run(TaskContext context);
}
#endif