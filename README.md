# Automatron

[![NuGet version (Automatron)](https://img.shields.io/nuget/v/Automatron.svg?style=flat-square)](https://www.nuget.org/packages/Automatron/)
[![Build status](https://dev.azure.com/lkt82/Public/_apis/build/status/Automatron%20CI?branchName=main)](https://dev.azure.com/lkt82/Public/_build/latest?definitionId=1)

The Automatron [.NET library](https://www.nuget.org/packages/Automatron) is a task automation system that enables you to write you build and deployment workflows in the same language as your .Net applications

Platform support: [.NET 6.0 and later](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6).

- [Quick start](#quick-start)
- [Tasks](#tasks)
- [Dependencies](#dependencies)
- [AzureDevOps](#azuredevops)

## Quick start

- Create a .NET console app named `Sample.Pipeline` and add a reference to [Automatron](https://www.nuget.org/packages/Automatron).
- Rename `Program.cs` to `Pipeline.cs` and replace the contents with:
```c#
using Automatron.Annotations;
using CommandDotNet;

namespace Sample.Pipeline;

public class Pipeline
{
    private readonly IConsole _console;

    public Pipeline(IConsole console)
    {
        _console = console;
    }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<Pipeline>().RunAsync(args);
    }

    public async Task Default()
    {
        await _console.Out.WriteLineAsync("Hello, world!");
    }
}
```
- Run the app. E.g. `dotnet run` or F5 in Visual Studio:

## Tasks

Tasks are defined as void/async methods. 

Inheritance and interfaces are supported

```c#
public class Pipeline
{
    private readonly IConsole _console;

    public Pipeline(IConsole console)
    {
        _console = console;
    }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<Pipeline>().RunAsync(args);
    }

    public async Task Default()
    {
        await _console.Out.WriteLineAsync("Hello, world!");
    }

    public void Build()
    {
        _console.Out.WriteLine("Building");
    }

    public async Task Test()
    {
        await _console.Out.WriteLineAsync("Testing");
    }
}
```

Tasks can be run via cmd arguments  ```dotnet run -- Build```

## Dependencies

Dependencies are expression via the attributes

- DependentOn 
- DependentFor

```c#
public class Pipeline
{
    private readonly IConsole _console;

    public Pipeline(IConsole console)
    {
        _console = console;
    }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<Pipeline>().RunAsync(args);
    }

    public async Task Default()
    {
        await _console.Out.WriteLineAsync("Hello, world!");
    }

    public void Build()
    {
        _console.Out.WriteLine("Building");
    }

    [DependentOn(nameof(Build))]
    public async Task Test()
    {
        await _console.Out.WriteLineAsync("Testing");
    }
}
```

## AzureDevOps

Automatron can be used for gennerating the yaml pipeline for AzureDevOps.

Pipelines are expression via the attributes in the addon library [Automatron.AzureDevOps](https://www.nuget.org/packages/Automatron.AzureDevOps).

*The project needs to be in git repostiory to able to compute paths correctly or have the RootPath property set*

```c#
[Pipeline]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
public class Pipeline
{
    private readonly IConsole _console;

    public Pipeline(IConsole console)
    {
        _console = console;
    }

    private static async Task<int> Main(string[] args)
    {
        return await new TaskRunner<Pipeline>().RunAsync(args);
    }

    [Stage]
    [Job]
    public void Ci() { }

    [AutomatronTask(nameof(Ci), SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Version))]
    public void Build()
    {
        _console.Out.WriteLine("Building");
    }

    [AutomatronTask(nameof(Ci), SkipDependencies = true)]
    [DependentFor(nameof(Ci))]
    [DependentOn(nameof(Build))]
    public async Task Test()
    {
        await _console.Out.WriteLineAsync("Testing");
    }
}
```

By reference the Automatron.AzureDevOps library and using the attributes on the class should result in a gennerated azure-pipelines.yml that is added to the project.
