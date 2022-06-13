using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using CommandDotNet;

namespace Automatron.AzureDevOps.Sample;

[Pipeline("../../", Cd)]
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
            await _console.Out.WriteLineAsync(AzureClientSecret);
        }

        [EnvVar("AZURE_CLIENT_SECRET")]
        [Option(Description = "one of the application's client secrets")]
        public string? AzureClientSecret { get; set; }
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

    //[Option(Description = "id of an Azure Active Directory application")]
    //public string? AzureClientId { get; set; }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<SamplePipeline2>().UseAzureDevOps().RunAsync(args);
    }
}