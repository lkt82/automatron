using Automatron.AzureDevOps.Annotations;

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
public class IntegrationTestingB
{
    public const string Environment = "Integration";

    [DeploymentJob("Setup", Environment = Environment)]
    public class SetupJob
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

    [DeploymentJob("Teardown", Environment = Environment, DependsOn = new[] { nameof(SetupJob) })]
    public class TeardownJob
    {
        [Step]
        public virtual void Init()
        {

        }

        [Step(DependsOn = new[] { nameof(Init) })]
        public virtual void Destroy()
        {

        }
    }
}