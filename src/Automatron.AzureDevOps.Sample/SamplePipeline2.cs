using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Sample
{
    [Pipeline("../../", Cd)]
    [CiTrigger(Cd, Batch = true, IncludeBranches = new[] { "main" })]
    public interface IContinuousDeployment
    {
        public const string Cd = "ContinuousDeployment2";

        public abstract class DeploymentTasks
        {
            private readonly AzureDevOpsTasks _azureDevOpsTasks;
            private readonly string _environment;

            protected DeploymentTasks(AzureDevOpsTasks azureDevOpsTasks,string environment)
            {
                _azureDevOpsTasks = azureDevOpsTasks;
                _environment = environment;
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
            public const string Environment = "Testing";

            public DeploymentTesting(AzureDevOpsTasks azureDevOpsTasks) : base(azureDevOpsTasks, Environment)
            {
            }

            [DeploymentJob(nameof(DeploymentTesting), Environment)]
            [AutomatronTask(DisplayName = nameof(Deployment), SkipDependencies = true)]
            [DependentOn(nameof(Deploy))]
            public void Deployment()
            {
            }
        }

        [StageTemplate(Cd,typeof(DeploymentTesting))]
        [DependentOn(typeof(DeploymentTesting), nameof(DeploymentTesting.Deploy))]
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

    }
}
