using System.Reflection;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;
using Automatron.Models;
using static SimpleExec.Command;

await new AzureDevOpsRunner().RunAsync(args);

[Pipeline("Ci",YmlPath = RootPath,YmlName = "azure-pipelines")]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job]
public class Pipeline
{
    private readonly LoggingCommands _loggingCommands;

    private const string RootPath = "../../";

    private const string Configuration = "Release";

    private static string RootDir => Path.GetFullPath(RootPath,Directory.GetCurrentDirectory());

    private static string ArtifactsDir => $"{RootDir}.artifacts";

    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    public Pipeline(LoggingCommands loggingCommands)
    {
        _loggingCommands = loggingCommands;
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

    [Step(Emoji = "🔢")]
    public async Task Version()
    {
        var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version == null)
        {
            return;
        }

        await _loggingCommands.UpdateBuildNumberAsync(version);
    }

    [Step(Emoji = "🧹")]
    public void Clean()
    {
        EnsureDirectory(ArtifactsDir);
        CleanDirectory(ArtifactsDir);
    }

    [Step(Emoji = "🏗", DependsOn = new []{ nameof(Version), nameof(Clean) })]
    public async Task Build()
    {
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron",noEcho:true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tasks", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.Tests", noEcho: true);
        await RunAsync("dotnet", $"dotnet build -c {Configuration}", workingDirectory: "../Automatron.AzureDevOps.Tests", noEcho: true);
    }

    [Step(Emoji = "🧪", DependsOn = new[] { nameof(Build), nameof(Clean) })]
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

        await _loggingCommands.PublishTestResultsAsync("XUnit", Directory.EnumerateFiles(ArtifactsDir, "*.Tests.xml"), "Tests", true);

        if (failedTests)
        {
            throw new Exception("Failed Unit Tests");
        }
    }

    [Step(Emoji = "📦", DependsOn = new[] { nameof(Build), nameof(Clean) })]
    public async Task Pack()
    {
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.AzureDevOps", noEcho: true);
        await RunAsync("dotnet", $"dotnet pack --no-build -c {Configuration} -o {ArtifactsDir}", workingDirectory: "../Automatron.Tasks", noEcho: true);
    }

    [Step(Emoji = "🚀", DependsOn = new[] { nameof(Pack) })]
    public async Task Publish()
    {
        foreach (var nuget in Directory.EnumerateFiles(ArtifactsDir, "*.nupkg"))
        {
            await _loggingCommands.UploadArtifactAsync("/", "Nuget", nuget);
            await _loggingCommands.UploadArtifactAsync("/", "Nuget", nuget.Replace("nupkg", "snupkg"));
            await RunAsync("dotnet", $"nuget push {nuget} -k {NugetApiKey?.GetValue()} -s https://api.nuget.org/v3/index.json --skip-duplicate", workingDirectory: "../Automatron", noEcho: true);
        }
    }
}