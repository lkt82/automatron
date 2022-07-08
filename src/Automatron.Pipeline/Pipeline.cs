using System.ComponentModel;
using System.Reflection;
using Automatron.Annotations;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Generators.Annotations;
using static SimpleExec.Command;

namespace Automatron.Pipeline;

[Pipeline(YmlPath = RootPath)]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
public class Pipeline
{
    private readonly AzureDevOpsTasks _azureDevOpsTasks;

    private const string RootPath = "../../";

    private const string Configuration = "Release";

    private static string RootDir => Path.GetFullPath(RootPath,Directory.GetCurrentDirectory());

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    [SecretVariable]
    [Description("The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    public Pipeline(AzureDevOpsTasks azureDevOpsTasks)
    {
        _azureDevOpsTasks = azureDevOpsTasks;
    }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<Pipeline>().UseAzureDevOps().RunAsync(args);
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

    [Stage]
    [Job]
    public void Ci() { }

    [AutomatronTask(nameof(Ci),Emoji = "🔢", SkipAll = true)]
    [DependentFor(nameof(Ci))]
    public async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await _azureDevOpsTasks.UpdateBuildNumber(version);
    }

    [AutomatronTask(nameof(Ci), Emoji = "🧹", SkipAll = true)]
    public void Clean()
    {
        EnsureDirectory(ArtifactsDir);
        CleanDirectory(ArtifactsDir);
    }

    [AutomatronTask(nameof(Ci), Emoji = "🏗", SkipAll = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Version), nameof(Clean))]
    public async Task Build()
    {
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron",noEcho:true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps.Tests", noEcho: true);
    }

    [AutomatronTask(nameof(Ci), Emoji = "🧪", SkipAll = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Build), nameof(Clean))]
    public async Task Test()
    {
        var failedTests = false;

        await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration} -r {ArtifactsDir} --collect:\"XPlat Code Coverage\" --logger:xunit;LogFileName=Automatron.Tests.xml", workingDirectory: "../Automatron.Tests", noEcho: true,handleExitCode: c=>
        {
            if (c == 0) return false;
            failedTests = true;
            return true;
        });

        await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration} -r {ArtifactsDir} --collect:\"XPlat Code Coverage\" --logger:xunit;LogFileName=Automatron.AzureDevOps.Tests.xml", workingDirectory: "../Automatron.AzureDevOps.Tests", noEcho: true, handleExitCode: c =>
        {
            if (c == 0) return false;
            failedTests = true;
            return true;
        });

        //await _azureDevOpsTasks.PublishTestResults("XUnit", Directory.EnumerateFiles(ArtifactsDir, "*.Tests.xml"), "Tests", true);

        if (failedTests)
        {
            throw new Exception("Failed Unit Tests");
        }
    }

    [AutomatronTask(nameof(Ci), Emoji = "📦", SkipAll = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Test), nameof(Clean))]
    public async Task Pack()
    {
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
    }

    [AutomatronTask(nameof(Ci), Emoji = "🚀", SkipAll = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Pack))]
    public async Task Publish()
    {
        //foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        //{
        //    await _azureDevOpsTasks.UploadArtifact("/", "Nuget", nuget);
        //    await _azureDevOpsTasks.UploadArtifact("/", "Nuget", nuget.Replace("nupkg", "snupkg"));
        //    await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey?.GetValue()} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: "../Automatron", noEcho: true);
        //}
    }
}