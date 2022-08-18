using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public sealed class Stage
{
    public Stage(Pipeline pipeline, string name, string? displayName, string[]? dependsOn, string? condition)
    {
        Name = name;
        DisplayName = displayName;
        DependsOn = dependsOn;
        Condition = condition;
        Pipeline = pipeline;

        Path = pipeline.Path + "/" + Name;
    }

    [YamlMember(Alias = "stage")]
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }

    public Pool? Pool { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    public IEnumerable<IJob>? Jobs { get; set; }

    [YamlIgnore]
    public Pipeline Pipeline { get; }

    [YamlIgnore] public string Path { get; set; }
}