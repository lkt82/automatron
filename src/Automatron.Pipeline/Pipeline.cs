using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;
using static SimpleExec.Command;

namespace Automatron.Pipeline
{
    [Pipeline("../../",
        YmlPath = "../../"
    )]
    [CiTrigger(
        Batch = true,
        IncludeBranches = new[] { "main" },
        IncludePaths = new[] { "src" }
    )]
    [Pool(VmImage = "ubuntu-latest")]
    public class Pipeline
    {
        private static async Task<int> Main(string[] args) => await new TaskRunner<Pipeline>().RunAsync(args);

        [Stage]
        [Job]
        public void Ci() { }

        [AutomatronTask(nameof(Ci),DisplayName =nameof(Build))]
        [DependentFor(nameof(Ci))]
        public void Build()
        {
            Run("dotnet", "dotnet build -c Release",workingDirectory: "../Automatron");
            Run("dotnet", "dotnet build -c Release", workingDirectory: "../Automatron.AzureDevOps");
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Build))]
        [DependsOn(nameof(Build))]
        [DependentFor(nameof(Ci))]
        public void Test()
        {
            //Run("dotnet", "dotnet test -c Release", workingDirectory: "../Automatron");
           // Run("dotnet", "dotnet test -c Release", workingDirectory: "../Automatron.AzureDevOps");
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack), SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Test))]
        public void Pack()
        {
            Run("dotnet", "dotnet pack --no-build -c Release", workingDirectory: "../Automatron");
            Run("dotnet", "dotnet pack --no-build -c Release", workingDirectory: "../Automatron.AzureDevOps");
        }

    }
}
