using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Sample
{
    [Pipeline(Cd)]
    [CiTrigger(Cd, Batch = true, IncludeBranches = new[] { "master" })]
    public abstract class BasePipeline
    {
        public const string Cd = "ContinuousDeployment";

        [Stage(SamplePipeline.Cd)]
        public void DefaultStage()
        {
        }

        [Job(nameof(DefaultStage))]
        public void DefaultJob()
        {
        }

        [Task(nameof(DefaultJob))]
        public void DefaultTask()
        {
        }
    }
}