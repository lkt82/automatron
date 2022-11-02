using Automatron.Tasks.Annotations;

namespace Automatron.Tests;

public class TaskControllerC
{
    [Task]
    public virtual void C()
    {

    }

    [Task]
    public class H
    {
        [Task]
        [DependentFor(typeof(H))]
        public class H2
        {
            [Task]
            [DependentFor(typeof(H2))]
            public virtual void A()
            {

            }
        }
    }
}