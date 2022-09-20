#if NET6_0

namespace Automatron.Tasks.Models;

public interface ITaskRunner
{
    public System.Threading.Tasks.Task Run(Task task);
}
#endif