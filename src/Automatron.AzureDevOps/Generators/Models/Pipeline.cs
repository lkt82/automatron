using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Pipeline
    {
        public Pipeline(string name,string? path, string ymlName, string ymlPath)
        {
            Name = name;
            Path = path;
            YmlName = string.IsNullOrEmpty(new FileInfo(ymlName).Extension) ? ymlName + ".yml" : ymlName;
            YmlPath = ymlPath;
        }

        [YamlIgnore]
        public string Name { get; }

        [YamlIgnore]
        public string? Path { get; }

        [YamlIgnore]
        public string YmlName { get; }

        [YamlIgnore]
        public string YmlPath { get; }

        [YamlMember(Alias = "trigger")]
        public ICiTrigger? CiTrigger { get; set; }

        public IList<ScheduledTrigger> Schedules { get; set; } = new List<ScheduledTrigger>();

        public IList<IVariable> Variables { get; set; } = new List<IVariable>();

        public IList<Stage> Stages { get; set; } = new List<Stage>();
    }
}