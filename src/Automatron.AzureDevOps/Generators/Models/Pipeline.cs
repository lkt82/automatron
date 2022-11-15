using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Pipeline
{
    public Pipeline(string name, string ymlName, string ymlDir, string rootDir,string projectDir, string command, ISymbol symbol)
    {
        Name = name;
        YmlName = ymlName;
        YmlDir = ymlDir;
        RootDir = rootDir;
        ProjectDir = projectDir;
        Symbol = symbol;

        Command = command;
    }

    [YamlIgnore]
    public string Name { get; }

    [YamlIgnore]
    public string YmlName { get; }

    [YamlIgnore]
    public string YmlDir { get; }

    [YamlIgnore]
    public string RootDir { get; }

    [YamlIgnore] public string Command { get; }

    [YamlIgnore]
    public string ProjectDir { get; }

    [YamlIgnore]
    public ISymbol Symbol { get; set; }

    public IEnumerable<Parameter>? Parameters { get; set; }

    [YamlMember(Alias = "trigger")]
    public ICiTrigger? CiTrigger { get; set; }

    public IEnumerable<ScheduledTrigger>? Schedules { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    public Pool? Pool { get; set; }

    public IEnumerable<Stage>? Stages { get; set; }
}