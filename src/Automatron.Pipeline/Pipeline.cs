using Automatron.Annotations;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Generators.Annotations;
using CommandDotNet;
using static SimpleExec.Command;

namespace Automatron.Pipeline
{
    [Pipeline(RootDir,
        YmlPath = RootDir
    )]
    [CiTrigger(
        Batch = true,
        IncludeBranches = new[] { "main" },
        IncludePaths = new[] { "src" }
    )]
    [Pool(VmImage = "ubuntu-latest")]
    [VariableGroup("nuget")]
    public class Pipeline
    {
        private readonly AzureDevOpsTasks _azureDevOpsTasks;

        private const string RootDir = "../../";

        private const string Configuration = "Release";

        private const string ArtifactsDir = $"{RootDir}.artifacts";

        private const string NugetApiKeyName = "NUGET_API_KEY";

        [EnvVar(NugetApiKeyName)]
        [Option(Description = "The nuget api key")]
        public string? NugetApiKey { get; set; }

        public Pipeline(AzureDevOpsTasks azureDevOpsTasks)
        {
            _azureDevOpsTasks = azureDevOpsTasks;
        }

        private static async Task<int> Main(string[] args)
        {
            return await new TaskRunner<Pipeline>().UseAzureDevOps().RunAsync(args);
        }

        [Stage]
        [Job]
        public void Ci() { }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Version))]
        [DependentFor(nameof(Ci))]
        public async Task Version()
        {
            await _azureDevOpsTasks.UpdateBuildNumberWithAssemblyInformationalVersion();
        }

        [AutomatronTask(nameof(Ci),DisplayName =nameof(Build), SkipDependencies = true)]
        [DependsOn(nameof(Version))]
        [DependentFor(nameof(Ci))]
        public async Task Build()
        {
            await RunAsync("dotnet", $"dotnet build -c {Configuration}",workingDirectory: "../Automatron",noEcho:true);
            await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
            await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        }

        private static void CleanDirectory(string dir)
        {
            var path = Path.GetFullPath(dir);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void EnsureDirectory(string dir)
        {
            var path = Path.GetFullPath(dir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Test), SkipDependencies = true)]
        [DependsOn(nameof(Build))]
        [DependentFor(nameof(Ci))]
        public async Task Test()
        {
            await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Pack), SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Test))]
        public async Task Pack()
        {
            EnsureDirectory(ArtifactsDir);
            CleanDirectory(ArtifactsDir);

            await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron", noEcho: true);
            await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        }

        [AutomatronTask(nameof(Ci), DisplayName = nameof(Publish),Secrets = new []{ NugetApiKeyName }, SkipDependencies = true)]
        [DependentFor(nameof(Ci))]
        [DependsOn(nameof(Pack))]
        public async Task Publish()
        {
            foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
            {
                await RunAsync("dotnet", $"nuget push {Path.GetFullPath(nuget)} -k {NugetApiKey} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: "../Automatron", noEcho: true);
            }
        }

    }
}
