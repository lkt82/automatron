using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public abstract class Step
{
    [YamlIgnore]
    public IJob Job { get; }

    protected Step(IJob job)
    {
        Job = job;
    }

    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    [YamlIgnore]
    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }
}