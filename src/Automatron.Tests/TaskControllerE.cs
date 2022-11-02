using Automatron.Tasks.Annotations;

namespace Automatron.Tests;

[Task(Action = nameof(Default))]
public class TaskControllerE
{
    public virtual void Default()
    {

    }
}