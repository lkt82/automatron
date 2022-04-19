using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using CommandDotNet;

namespace Automatron.AzureDevOps.Sample
{
    [Pipeline(Cd)]
    [CiTrigger(Cd,
        Batch = true,
        IncludeBranches = new []{ "master" }
        )]

    [Pipeline(Pr)]
    [CiTrigger(Pr,Disabled = true)]
    [VariableGroup(Cd,"pulumi")]
    [Variable(Cd,"test",1)]
    public class SamplePipeline : BasePipeline, ICompositeIntegrationTesting
    {
        public const string Cd = "ContinuousDeployment";

        public const string CdPath = "Infrastructure";

        public const string CdYmlName = "ContinuousDeployment";

        public const string CdYmlPath = ".azure";

        public const string Pr = "PullRequest";

        private static async Task<int> Main(string[] args) => await new TaskRunner<SamplePipeline>().RunAsync(args);

        [Option(Description = "test")]
        public string? Param1 { get; set; }

        [Option(Description = "test", Split = ',')]
        public IEnumerable<string> Param2 { get; set; } = Enumerable.Empty<string>();

        [Stage(Pr)]

        public void PrStage()
        {
        }

        [Job(nameof(PrStage))]
        public void PrJob()
        {
        }

        [AutomatronTask(nameof(PrJob))]
        public void PrTask()
        {
        }

        [Stage(Cd)]
        public void CiStage()
        {
        }

        [DeploymentJob(nameof(CiStage),"test")]
        [DownloadTask(DownloadSource.None)]
        [CheckoutTask(CheckoutSource.Self)]
        [NuGetAuthenticateTask]
        [PulumiTask(DisplayName = "Install Pulumi")]
        [DependentFor(nameof(CiStage))]
        public void CiJob1()
        {
        }

        [Job(nameof(CiStage))]
        [DependsOn(nameof(RunStep2))]
        [DependentFor(nameof(CiStage))]
        public void CiJob2()
        {
        }

        [AutomatronTask(nameof(CiJob1))]
        [DependsOn(nameof(Clean))]
        [DependentFor(nameof(CiJob1))]
        public void RunStep1()
        {
        }

        [AutomatronTask(nameof(CiJob1),DisplayName = "Step 3")]
        [DependsOn(nameof(RunStep1),nameof(Build))]
        [DependentFor(nameof(CiJob1))]
        public void RunStep3()
        {
        }

        [AutomatronTask(nameof(CiJob2))]
        public void RunStep2()
        {
        }

        public void Clean()
        {
            Console.WriteLine("cleaing");
        }

        //[Stage]
        //[Job]
        //[BullseyeTask]
        [DependsOn(nameof(Clean))]
        public void Build()
        {
            //throw new Exception("fejl");

            //Debugger.Launch();

            Console.WriteLine($"bulding {Param1}");

            foreach (var parameter in Param2)
            {
                Console.WriteLine(parameter);
            }

            //throw new Exception();
        }
    }
}
