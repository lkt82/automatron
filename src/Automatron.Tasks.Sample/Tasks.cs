using Automatron.Tasks.Annotations;

namespace Automatron.Tasks.Sample;

public class Tasks
{
    private readonly Parameters _parameters;

    public Tasks(Parameters parameters)
    {
        _parameters = parameters;
    }

    [Parameter]
    public string? Value1 { get; set; }

    [Task]
    [DependentOn(nameof(Build))]
    public void Test()
    {
        Console.WriteLine(Value1);
        Console.WriteLine(_parameters.Value2);
        Console.WriteLine(_parameters.Value3);
    }

    [Task(Default = true)]
    public void Build()
    {

    }


    [Task]
    [DependentFor(nameof(Build))]
    public void Version()
    {

    }
}