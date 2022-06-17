using System.Reflection;
using Automatron.Annotations;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Generators.Annotations;
using static SimpleExec.Command;

namespace Automatron.Pipeline;

[Pipeline(RootDir, YmlPath = RootDir)]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
public class Pipeline
{
    private readonly AzureDevOpsTasks _azureDevOpsTasks;

    private const string RootDir = "../../";

    private const string Configuration = "Release";

    private const string ArtifactsDir = $"{RootDir}.artifacts";

    [Parameter("The nuget api key")]
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

    [AutomatronTask(nameof(Ci))]
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

    [AutomatronTask(nameof(Ci), SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Version))]
    public async Task Build()
    {
        await RunAsync("dotnet", $"dotnet build -c {Configuration}",workingDirectory: "../Automatron",noEcho:true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
    }

    [AutomatronTask(nameof(Ci), SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Build))]
    public async Task Test()
    {
        await RunAsync("dotnet", $"dotnet test --no-build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
    }

    [AutomatronTask(nameof(Ci), SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Test))]
    public async Task Pack()
    {
        EnsureDirectory(ArtifactsDir);
        CleanDirectory(ArtifactsDir);

        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
    }

    [AutomatronTask(nameof(Ci),Secrets = new []{ nameof(NugetApiKey) }, SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Pack))]
    public async Task Publish()
    {
        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await _azureDevOpsTasks.UploadArtifact("/", "Nuget", Path.GetFullPath(nuget));
            await _azureDevOpsTasks.UploadArtifact("/", "Nuget", Path.GetFullPath(nuget.Replace("nupkg", "snupkg")));
            await RunAsync("dotnet", $"nuget push {Path.GetFullPath(nuget)} -k {NugetApiKey?.GetValue()} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: "../Automatron", noEcho: true);
        }
    }
}