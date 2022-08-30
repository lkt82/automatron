using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public sealed class Job: IJob
{
    public Job(Stage stage,string name, string? displayName, string[]? dependsOn, string? condition, ISymbol symbol)
    {
        Name = name;
        DisplayName = displayName;
        DependsOn = dependsOn;
        Condition = condition;
        Symbol = symbol;
        Stage = stage;

        Path = Stage.Path + "/" + Name;
    }

    [YamlMember(Alias = "job")]
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }

    public Pool? Pool { get; set; }

    [YamlIgnore]
    public ISymbol Symbol { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    [YamlIgnore] public IDictionary<string, object>? Parameters { get; set; }

    public IEnumerable<Step>? Steps { get; set; }

    [YamlIgnore]
    public Stage Stage { get; }

    [YamlIgnore] 
    public string Path { get; set; }
}