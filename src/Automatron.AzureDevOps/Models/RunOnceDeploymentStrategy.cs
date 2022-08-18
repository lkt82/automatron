using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public sealed class RunOnceDeploymentStrategy: IDeploymentStrategy
{
    public RunOnceDeployment RunOnce { get; set; } = new();

    [YamlIgnore]
    public IEnumerable<Step>? Steps
    {
        get => RunOnce.Deploy.Steps;
        set => RunOnce.Deploy.Steps = value;
    }
}