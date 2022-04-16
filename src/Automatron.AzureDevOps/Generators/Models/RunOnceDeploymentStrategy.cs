using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class RunOnceDeploymentStrategy: IDeploymentStrategy
    {
        public RunOnceDeployment RunOnce { get; set; } = new();

        [YamlIgnore]
        public IList<Step> Steps => RunOnce.Deploy.Steps;
    }
}