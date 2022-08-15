#if NET6_0
using System.Threading.Tasks;

namespace Automatron;

internal static class TaskEngineExtensions
{
    public static Task<int> RunAsync(this ITaskEngine taskEngine, params string[] tasks)
    {
        return taskEngine.RunAsync(tasks);
    }
}
#endif