using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Pipeline
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

        [YamlMember(Alias = "trigger")]
        public ICiTrigger? CiTrigger { get; set; }

        public IList<ScheduledTrigger> Schedules { get; set; } = new List<ScheduledTrigger>();

        public IList<IVariable> Variables { get; set; } = new List<IVariable>();

        public Pool? Pool { get; set; }

        public IList<Stage> Stages { get; set; } = new List<Stage>();
    }
}