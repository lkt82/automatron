using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public sealed class Pipeline
{
    public Pipeline(string name, string ymlName, string ymlPath, string rootPath,string projectDirectory)
    {
        Name = name;
        YmlName = ymlName;
        YmlPath = ymlPath;
        RootPath = rootPath;
        ProjectDirectory = projectDirectory;

        Path = "/" + Name;
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

    [YamlIgnore]
    public List<string> Secrets { get; set; } = new();

    public List<Parameter>? Parameters { get; set; } = new();

    [YamlMember(Alias = "trigger")]
    public ICiTrigger? CiTrigger { get; set; }

    public IEnumerable<ScheduledTrigger>? Schedules { get; set; }

    public IEnumerable<IVariable>? Variables { get; set; }

    public Pool? Pool { get; set; }

    public IEnumerable<Stage>? Stages { get; set; }

    [YamlIgnore] public string Path { get; set; }
}