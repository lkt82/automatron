#if NETSTANDARD2_0
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Job: IJob
{
    public Job(Stage stage,string name, string? displayName, string[]? dependsOn, string? condition)
    {
        Name = name;
        DisplayName = displayName;
        DependsOn = dependsOn;
        Condition = condition;
        Stage = stage;
    }

    [YamlMember(Alias = "job")]
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }

    public Pool? Pool { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    [YamlIgnore] public IDictionary<string, object>? TemplateValues { get; set; }

    public IEnumerable<Step>? Steps { get; set; }

    [YamlIgnore]
    public Stage Stage { get; }
}
#endif