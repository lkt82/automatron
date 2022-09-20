using Automatron.Tasks.Annotations;

namespace Automatron.Tasks.Sample;

public class Parameters
{
    [Parameter]
    public string? Value2 { get; set; }

    [Parameter]
    public string? Value3 { get; set; }
}