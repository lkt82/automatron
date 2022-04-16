using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Sample
{
    public abstract class BasePipeline
    {
        [Stage(SamplePipeline.Cd)]
        public void DefaultStage()
        {
        }

        [Job(nameof(DefaultStage))]
        public void DefaultJob()
        {
        }

        [AutomatronTask(nameof(DefaultJob))]
        public void DefaultTask()
        {
        }
    }
}