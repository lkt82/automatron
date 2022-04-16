using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class DeploymentJob : IJob
    {
        public DeploymentJob(string name, string? displayName, string[]? dependsOn, string environment)
        {
            Name = name;
            DisplayName = displayName;
            DependsOn = dependsOn;
            Environment = environment;
        }

        [YamlMember(Alias = "deployment")]
        public string Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public int? TimeoutInMinutes { get; set; }

        public string Environment { get; }

        public IDeploymentStrategy Strategy { get; set; } = new RunOnceDeploymentStrategy();

        [YamlIgnore] public IList<Step> Steps => Strategy.Steps;
    }
}