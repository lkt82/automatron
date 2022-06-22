using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class RunOnceDeploymentStrategy: IDeploymentStrategy
{
    public RunOnceDeployment RunOnce { get; set; } = new();

    [YamlIgnore]
    public List<Step> Steps => RunOnce.Deploy.Steps;
}