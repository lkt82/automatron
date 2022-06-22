using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Pipeline
{
    public Pipeline(string name, string ymlName, string ymlPath, string rootPath,string projectDirectory)
    {
        Name = name;
        YmlName = string.IsNullOrEmpty(new FileInfo(ymlName).Extension) ? ymlName + ".yml" : ymlName;
        YmlPath = ymlPath;
        RootPath = rootPath;
        ProjectDirectory = projectDirectory;
    }

    [YamlIgnore]
    public string Name { get; }

    [YamlIgnore]
    public string YmlName { get; }

    [YamlIgnore]
    public string YmlPath { get; }

    [YamlIgnore]
    public string RootPath { get; }

    [YamlIgnore]
    public string ProjectDirectory { get; }

    public List<Parameter> Parameters { get; set; } = new();

    [YamlMember(Alias = "trigger")]
    public ICiTrigger? CiTrigger { get; set; }

    public List<ScheduledTrigger> Schedules { get; set; } = new();

    public List<IVariable> Variables { get; set; } = new();

    public Pool? Pool { get; set; }

    public List<Stage> Stages { get; set; } = new();
}