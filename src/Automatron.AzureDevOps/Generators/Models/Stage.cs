﻿#if NETSTANDARD2_0
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Stage
{
    public Stage(Pipeline pipeline, string name, string? displayName, string[]? dependsOn, string? condition, ISymbol symbol)
    {
        Name = name;
        DisplayName = displayName;
        DependsOn = dependsOn;
        Condition = condition;
        Symbol = symbol;
        Pipeline = pipeline;
    }

    [YamlMember(Alias = "stage")]
    public string Name { get; set; }

    public string? DisplayName { get; set; }

    public string[]? DependsOn { get; set; }

    public string? Condition { get; set; }

    public Pool? Pool { get; set; }

    [YamlIgnore]
    public ISymbol Symbol { get; set; }

    [YamlIgnore]
    public IDictionary<string,object>? TemplateParameters { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    public IEnumerable<IJob>? Jobs { get; set; }

    [YamlIgnore]
    public Pipeline Pipeline { get; }
}
#endif