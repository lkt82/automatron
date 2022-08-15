using Automatron;
using Automatron.Annotations;

await new TaskRunner().RunAsync(args);

public class Tasks
{
    private readonly Parameters _parameters;

    public Tasks(Parameters parameters)
    {
        _parameters = parameters;
    }

    [Parameter]
    public string? Value3 { get; set; }

    [Task(Default = true)]
    public void Build()
    {

    }

    [Task]
    public class NestedTasks
    {
        private readonly Parameters _parameters;

        [Parameter]
        public string? Value4 { get; set; }

        public NestedTasks(Parameters parameters)
        {
            _parameters = parameters;
        }

        [Task]
        [DependentFor(typeof(NestedTasks))]
        public void Build2()
        {
            Console.WriteLine("hello");
            Console.WriteLine(Value4);
            Console.WriteLine(_parameters.Value1);
        }
    }
}

public class Parameters
{
    [Parameter]
    public string? Value1 { get; set; }

    [Parameter]
    public string? Value2 { get; set; }
}