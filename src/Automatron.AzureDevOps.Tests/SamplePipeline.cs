using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Tests
{
    [Pipeline(Cd)]
    [CiTrigger(Cd, Batch = true, IncludeBranches = new[] { "main" })]
    public interface IContinuousDeployment
    {
        public const string Cd = "ContinuousDeployment";

        public abstract class DeploymentTasks
        {
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

    public class SamplePipeline : IContinuousDeployment
    {
        public const string Cd = "ContinuousDeployment2";
    }
}
