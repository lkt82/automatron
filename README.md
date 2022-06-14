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

Dependencies are expression via the attributes DependentOn and DependentFor

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
