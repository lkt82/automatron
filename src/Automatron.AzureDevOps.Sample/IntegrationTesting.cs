using System.ComponentModel;
using System.Linq.Expressions;
using Automatron.AzureDevOps.Annotations;

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

    [Variable]
    [Description("The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    [Variable]
    public virtual string? AzureClientId { get; set; }

    [Stage("Integration")]
    [Variable("NUGET.PLUGIN.HANDSHAKE.TIMEOUT.IN.SECONDS", "60")]
    [Variable("NUGET.PLUGIN.REQUEST.TIMEOUT.IN.SECONDS", "60")]
    [VariableGroup("Test")]
    [Parameter(nameof(RunTests), Value = "${{" + nameof(RunTests) + "}}")]
    public class IntegrationStage
    {
        [Variable]
        public virtual string? AzureClientId { get; set; }

        [DeploymentJob("Setup", Environment = "Integration")]
        [Parameter(nameof(RunTests),Value = "${{"+nameof(IntegrationTesting.RunTests) +"}}")]
        public class SetupJob
        {
            private readonly IntegrationStage _stage;
            private readonly IntegrationTesting _integrationTesting;

            public SetupJob(IntegrationStage stage, IntegrationTesting integrationTesting)
            {
                _stage = stage;
                _integrationTesting = integrationTesting;
            }

            [Environment]
            public virtual string? Environment { get; set; }

            [Parameter]
            public string? RunTests { get; set; }

            [Variable]
            public virtual string? AzureClientId { get; set; }

            [Step]
            public virtual void Init()
            {

            }

            [Step(DependsOn = new[] { nameof(Init) })]
            public virtual void Update()
            {

            }
        }

        [DeploymentJob("Teardown", Environment = "Integration", DependsOn = new object[]{typeof(SetupJob) } )]
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