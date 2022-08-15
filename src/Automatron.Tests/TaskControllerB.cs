using Automatron.Annotations;

namespace Automatron.Tests;

public class TaskControllerB
{
    [Task]
    [DependentOn(typeof(TaskControllerA),nameof(TaskControllerA.A_E))]
    public virtual void B_A() { }

    [Task]
    [DependentOn(typeof(TaskControllerE))]
    public virtual void B_B() { }
}