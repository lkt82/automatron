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

        private string _configuration = "Release";

        [EnvVar("NUGET_API_KEY")]
        [Option("nugetApiKey",Description = "The nuget api key")]
        public string? NugetApiKey { get; set; }

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
            await RunAsync("dotnet", $"dotnet build -c {_configuration}",workingDirectory: "../Automatron",noEcho:true);
            await RunAsync("dotnet", $"dotnet build -c {_configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
            await RunAsync("dotnet", $"dotnet build -c {_configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Test), SkipDependencies = true)]
        [DependsOn(nameof(Build))]
        [DependentFor(nameof(Ci))]
        public async Task Test()
        {
            await RunAsync("dotnet", $"dotnet test --no-build -c {_configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack), SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Test))]
        public async Task Pack()
        {
            await RunAsync("dotnet", $"dotnet pack --no-build -c {_configuration}", workingDirectory: "../Automatron", noEcho: true);
            await RunAsync("dotnet", $"dotnet pack --no-build -c {_configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack),Secrets = new []{ "NUGET_API_KEY" }, SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Pack))]
        public async Task Publish()
        {
            foreach (var nuget in Directory.EnumerateFiles($"../Automatron/bin/{_configuration}", "*.nupkg"))
            {
                await RunAsync("dotnet", $"nuget push {Path.GetFullPath(nuget)} -k {NugetApiKey} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: "../Automatron", noEcho: false);
            }
        }

    }
}
