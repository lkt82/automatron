using Automatron.Tasks.Annotations;

namespace Automatron.Tests;

public abstract class TaskControllerFBase
{
    [Parameter]
    public string? Value { get; set; }

    [Task]
    public virtual void F() { }
}

[Task]
public class TaskControllerF1: TaskControllerFBase
{
}

[Task]
public class TaskControllerF2: TaskControllerFBase
{
}