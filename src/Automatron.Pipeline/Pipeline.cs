using Automatron.Annotations;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Generators.Annotations;
using CommandDotNet;
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
        private readonly IConsole _console;

        public Pipeline(IConsole console)
        {
            _console = console;
        }

        private static async Task<int> Main(string[] args) => await new TaskRunner<Pipeline>().RunAsync(args);

        [Stage]
        [Job]
        public void Ci() { }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Version))]
        [DependentFor(nameof(Ci))]
        public async Task Version()
        {
            await _console.UpdateBuildNumberWithAssemblyInformationalVersion();
        }

        [AutomatronTask(nameof(Ci),DisplayName =nameof(Build), SkipDependencies = true)]
        [DependsOn(nameof(Version))]
        [DependentFor(nameof(Ci))]
        public async Task Build()
        {
            await RunAsync("dotnet", "dotnet build -c Release",workingDirectory: "../Automatron");
            await RunAsync("dotnet", "dotnet build -c Release", workingDirectory: "../Automatron.AzureDevOps");
            await RunAsync("dotnet", "dotnet build -c Release", workingDirectory: "../Automatron.Tests");
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Test), SkipDependencies = true)]
        [DependsOn(nameof(Build))]
        [DependentFor(nameof(Ci))]
        public async Task Test()
        {
            await RunAsync("dotnet", "dotnet test --no-build -c Release", workingDirectory: "../Automatron.Tests");
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack), SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Test))]
        public async Task Pack()
        {
            await RunAsync("dotnet", "dotnet pack --no-build -c Release", workingDirectory: "../Automatron");
            await RunAsync("dotnet", "dotnet pack --no-build -c Release", workingDirectory: "../Automatron.AzureDevOps");
        }

    }
}
