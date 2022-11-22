using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class DeploymentJob : IJob
{
    public DeploymentJob(Stage stage,string name, string? displayName, string[]? dependsOn, string? condition,string environment, ISymbol symbol)
    {
        Name = name;
        DisplayName = displayName;
        DependsOn = dependsOn;
        Condition = condition;
        Environment = environment;
        Symbol = symbol;
        Stage = stage;
    }

    [YamlMember(Alias = "deployment")]
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }

    public Pool? Pool { get; set; }

    [YamlIgnore]
    public ISymbol Symbol { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    [YamlIgnore] public IDictionary<string, object>? TemplateParameters { get; set; }

    public int? TimeoutInMinutes { get; set; }

    public string Environment { get; }

    public IDeploymentStrategy Strategy { get; set; } = new RunOnceDeploymentStrategy();

    [YamlIgnore] public IEnumerable<Step>? Steps
    {
        get => Strategy.Steps;
        set => Strategy.Steps = value;
    }

    [YamlIgnore]
    public Stage Stage { get; }
}