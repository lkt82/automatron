using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Stage
    {
        public Stage(string name, string? displayName, string[]? dependsOn, string? condition)
        {
            Name = name;
            DisplayName = displayName;
            DependsOn = dependsOn;
            Condition = condition;
        }

        [YamlMember(Alias = "stage")]
        public string Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public IList<IJob> Jobs { get; set; } = new List<IJob>();
    }
}
