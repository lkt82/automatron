using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Job: IJob
    {
        public Job(string name, string? displayName, string[]? dependsOn)
        {
            Name = name;
            DisplayName = displayName;
            DependsOn = dependsOn;
        }

        [YamlMember(Alias = "job")]
        public string Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public IList<Step> Steps { get; set; } = new List<Step>();
    }
}
