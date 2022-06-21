using System.ComponentModel;
using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using CommandDotNet;
using Parameter = Automatron.AzureDevOps.Generators.Annotations.ParameterAttribute;

namespace Automatron.AzureDevOps.Sample;

[Pipeline(Cd)]
[CiTrigger(Cd, Batch = true, IncludeBranches = new[] { "main" })]
public interface IContinuousDeployment
{
    public const string Cd = "ContinuousDeployment2";

    public abstract class DeploymentTasks
    {
        protected string Environment { get; }
        private readonly AzureDevOpsTasks _azureDevOpsTasks;

        protected DeploymentTasks(AzureDevOpsTasks azureDevOpsTasks,string environment)
        {
            _azureDevOpsTasks = azureDevOpsTasks;

            Environment = environment;
        }

        [AutomatronTask]
        public void Build()
        {
        }


        [AutomatronTask(SkipDependencies = true)]
        [DependentOn(nameof(Build))]
        public void Deploy()
        {
        }

        [Description("The Azure AD application's client secret")]
        public Secret? AzureClientSecret { get; set; }
    }

    public class DeploymentTesting : DeploymentTasks
    {
        private readonly IConsole _console;
        public const string Name = "Testing";

        public DeploymentTesting(AzureDevOpsTasks azureDevOpsTasks,IConsole console) : base(azureDevOpsTasks, Name)
        {
            _console = console;
        }

        [DeploymentJob(nameof(Name))]
        [Pool(VmImage = "ubuntu-latest")]
        [DependentOn(nameof(Deploy))]
        public async Task Deployment()
        {
            _console.WriteLine(Environment);
            await _console.Out.WriteLineAsync(AzureClientSecret?.GetValue());
        }
    }

    [Stage(Cd,typeof(DeploymentTesting))]
    [DependentOn(typeof(DeploymentTesting), nameof(DeploymentTesting.Deployment))]
    public void DeployToTesting()
    {
    }
}

public class SamplePipeline2 : IContinuousDeployment
{
    public const string Cd = "ContinuousDeployment2";

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<SamplePipeline2>().UseAzureDevOps().RunAsync(args);
    }

    [Parameter(Cd, "runPerfTests", ParameterTypes.Boolean, Value = false)]
    public bool RunPerfTests { get; set; }

    //[AutomatronTask(nameof(Default))]
    //[Stage(Cd, DependsOn = new[] { nameof(IContinuousDeployment.DeployToTesting) })]
    //[Job(nameof(IContinuousDeployment.DeployToTesting))]
    [DependentFor(typeof(IContinuousDeployment.DeploymentTesting),nameof(IContinuousDeployment.DeploymentTesting.Deployment))]
    public void Default()
    {
    }
}