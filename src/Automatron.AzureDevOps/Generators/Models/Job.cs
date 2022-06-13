using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Job: IJob
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

        public List<Step> Steps { get; set; } = new();

        [YamlIgnore]
        public Stage Stage { get; }

        [YamlIgnore]
        public string? Template { get; set; }
    }
}
