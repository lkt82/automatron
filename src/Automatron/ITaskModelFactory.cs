#if NET6_0
namespace Automatron;

internal interface ITaskModelFactory
{
    public TaskModel Create();
}

#endif