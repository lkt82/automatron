using Automatron.Annotations;

namespace Automatron.Tests;

public class TaskControllerA
{
    [Parameter]
    public string Value1 { get; set; }

    [Task(Default = true)]
    public virtual void Default() { }

    [Task]
    public virtual void A_Test() { }

    [Task]
    public virtual void A() { }

    [Task]
    [DependentOn(nameof(A))]
    public virtual void A_B() { }

    [Task]
    [DependentOn(nameof(A_B))]
    public virtual void A_C() { }

    [Task]
    [DependentOn(nameof(A_C))]
    [DependentFor(nameof(A_E))]
    public virtual void A_D() { }

    [Task]
    public virtual void A_E() { }
}