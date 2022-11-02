#if NET6_0

namespace Automatron.Tasks.Models;

public interface ITaskEngine
{
    public System.Threading.Tasks.Task Run(Task task);
}
#endif