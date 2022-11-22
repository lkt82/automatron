using Automatron.AzureDevOps.Annotations;
using Automatron.Models;

namespace Automatron.AzureDevOps.Sample;

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
public class IntegrationTesting
{
    [Parameter(DisplayName = "Run Tests", Values = new object[] { "Yes", "No" })]
    public string? RunTests { get; set; }

    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    [Variable(Value = "AZURE_CLIENT_ID")]
    public virtual string? AzureClientId { get; set; }

    [Stage("Integration")]
    [Variable("NUGET.PLUGIN.HANDSHAKE.TIMEOUT.IN.SECONDS", "60")]
    [Variable("NUGET.PLUGIN.REQUEST.TIMEOUT.IN.SECONDS", "60")]
    [VariableGroup("Test")]
    public class IntegrationStage
    {
        public IntegrationStage(IntegrationTesting integrationTesting)
        {

        }

        [DeploymentJob("Setup", Environment = "Integration")]
        public class SetupJob
        {
            private readonly IntegrationStage _stage;

            public SetupJob(IntegrationStage stage)
            {
                _stage = stage;
            }

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

        [DeploymentJob("Teardown", Environment = "Integration", DependsOn = new []{ "Setup" } )]
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