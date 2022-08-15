using Automatron.Annotations;

namespace Automatron.Tests;

public abstract class TaskControllerFBase
{
    [Parameter]
    public string? Value { get; set; }

    [Task]
    public virtual void F() { }

    [Task(Action = nameof(F2A))]
    public class F2
    {
        public virtual void F2A()
        {

        }
    }
}

[Task]
public class TaskControllerF1: TaskControllerFBase
{
}

[Task]
public class TaskControllerF2: TaskControllerFBase
{
}