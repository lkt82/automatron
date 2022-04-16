using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Stage
    {
        public Stage(string name, string? displayName, string[]? dependsOn)
        {
            Name = name;
            DisplayName = displayName;
            DependsOn = dependsOn;
        }

        [YamlMember(Alias = "stage")]
        public string Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public IList<IJob> Jobs { get; set; } = new List<IJob>();
    }
}
