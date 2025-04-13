#if NETSTANDARD2_0
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

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
#endif