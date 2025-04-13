using System.Reflection;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using static SimpleExec.Command;

namespace Automatron.Pipeline;

[Pipeline("Ci",YmlDir = RelativeRootDir,YmlName = "azure-pipelines")]
[CiTrigger(Batch = true, IncludeBranches = ["main"], IncludePaths = ["src"])]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job]
public class Pipeline(LoggingCommands loggingCommands)
{
    private const string RelativeRootDir = "../../";

    private const string Configuration = "Release";

    private static string RootDir => Path.GetFullPath(RelativeRootDir,Directory.GetCurrentDirectory());

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

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

    [Checkout(CheckoutSource.Self, FetchDepth = 0)]
    [Step(Emoji = "🔢")]
    public async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await loggingCommands.UpdateBuildNumberAsync(version);
    }

    [Step(Emoji = "🧹")]
    public void Clean()
    {
        CleanDirectory(ArtifactsDir);
        EnsureDirectory(ArtifactsDir);
    }

    [Step(Emoji = "🏗", DependsOn = [nameof(Version), nameof(Clean)])]
    public async Task Build()
    {
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron",noEcho:true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps.Tests", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tasks", noEcho: true);
    }

    [Step(Emoji = "🧪", DependsOn = [nameof(Build), nameof(Clean)])]
    public async Task Test()
    {
        var failedTests = false;

        await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration} --results-directory {ArtifactsDir} --collect:\"XPlat Code Coverage\" --logger:xunit;LogFileName=Automatron.Tests.xml", workingDirectory: "../Automatron.Tests", noEcho: true,handleExitCode: c=>
        {
            if (c == 0) return false;
            failedTests = true;
            return true;
        });

        await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration} --results-directory {ArtifactsDir} --collect:\"XPlat Code Coverage\" --logger:xunit;LogFileName=Automatron.AzureDevOps.Tests.xml", workingDirectory: "../Automatron.AzureDevOps.Tests", noEcho: true, handleExitCode: c =>
        {
            if (c == 0) return false;
            failedTests = true;
            return true;
        });

        await loggingCommands.PublishTestResultsAsync("XUnit", Directory.EnumerateFiles(ArtifactsDir, "*.Tests.xml"), "Tests", true);

        if (failedTests)
        {
            throw new Exception("Failed Unit Tests");
        }
    }

    [Step(Emoji = "📦", DependsOn = [nameof(Build), nameof(Clean)])]
    public async Task Pack()
    {
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.Tasks", noEcho: true);
    }

    [Step(Emoji = "🚀", DependsOn = [nameof(Pack)])]
    public async Task Publish()
    {
        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await loggingCommands.UploadArtifactAsync("/", "Nuget", nuget);
            await loggingCommands.UploadArtifactAsync("/", "Nuget", nuget.Replace("nupkg", "snupkg"));
            await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey?.GetValue()} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: RootDir, noEcho: true);
        }
    }
}