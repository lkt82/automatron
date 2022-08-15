using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Tests;

[Pipeline("Int", YmlName = "IntegrationTesting")]
[CiTrigger(Disabled = true)]
[ScheduledTrigger(
    "2 0 * * *",
    DisplayName = "Midnight",
    IncludeBranches = new[] { "master" }
)]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[DeploymentJob(Environment = "Integration")]
public class IntegrationTestingC
{
    [Step]
    public virtual void Init()
    {

    }

    [Step(DependsOn = new[] { nameof(Init) })]
    public virtual void Update()
    {

    }
}