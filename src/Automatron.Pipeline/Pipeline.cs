using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.Pipeline
{
    [Pipeline("../../",
        YmlPath = "../../"
    )]
    [CiTrigger(
        Batch = true,
        IncludeBranches = new[] { "main" },
        IncludePaths = new[] { "src2" }
    )]
    [Pool(VmImage = "ubuntu-latest")]
    public class Pipeline
    {
        private static async Task<int> Main(string[] args) => await new TaskRunner<Pipeline>().RunAsync(args);

        [Stage]
        [Job]
        public void Ci() { }

        [AutomatronTask(nameof(Ci))]
        [DependentFor(nameof(Ci))]
        public void Build()
        {

        }

        [AutomatronTask(nameof(Ci))]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Build))]
        public void Pack()
        {

        }

    }
}
