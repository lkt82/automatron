using Automatron.Annotations;

namespace Automatron.Tests;

public interface ITaskControllerD
{
    [Task]
    public virtual void D_A()
    {

    }
}

public class TaskControllerD : ITaskControllerD
{

}