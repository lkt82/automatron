using System.ComponentModel;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Tests;

[Pipeline("Int",YmlName = "IntegrationTesting")]
[CiTrigger(Disabled = true)]
[ScheduledTrigger(
    "2 0 * * *",
    DisplayName = "Midnight",
    IncludeBranches = new[] { "master" }
)]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("Nuget")]
[VariableGroup("Pulumi")]
[VariableGroup("Azure")]
[Variable("NUGET.PLUGIN.HANDSHAKE.TIMEOUT.IN.SECONDS", "30")]
[Variable("NUGET.PLUGIN.REQUEST.TIMEOUT.IN.SECONDS", "30")]
public class IntegrationTestingA
{
    [Variable]
    [Description("The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    [Variable]
    public virtual string? AzureClientId { get; set; }

    [Stage("Integration")]
    [Variable("NUGET.PLUGIN.HANDSHAKE.TIMEOUT.IN.SECONDS", "60")]
    [Variable("NUGET.PLUGIN.REQUEST.TIMEOUT.IN.SECONDS", "60")]
    [VariableGroup("Test")]
    public class IntegrationStage
    {
        [DeploymentJob("Setup", Environment = "Integration")]
        public class SetupJob
        {
            [Environment]
            public virtual string? Environment { get; set; }

            [Step]
            public virtual void Init()
            {

            }

            [Step(DependsOn = new[] { nameof(Init) })]
            public virtual void Update()
            {

            }
        }

        [DeploymentJob("Teardown", Environment = "Integration", DependsOn = new []{typeof(SetupJob) } )]
        public class TeardownJob
        {
            [Environment]
            public virtual string? Environment { get; set; }

            [Step]
            public virtual void Init()
            {

            }

            [Step(DependsOn = new []{ nameof(Init) })]
            public virtual void Destroy()
            {

            }
        }
    }
}