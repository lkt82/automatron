# Automatron.AzureDevOps
[![NuGet version (Automatron.AzureDevOps)](https://img.shields.io/nuget/v/Automatron.AzureDevOps.svg?style=flat-square)](https://www.nuget.org/packages/Automatron.AzureDevOps/)
[![Build status](https://dev.azure.com/lkt82/Public/_apis/build/status/Automatron%20CI?branchName=main)](https://dev.azure.com/lkt82/Public/_build/latest?definitionId=1)

The Automatron.AzureDevOps [.NET library](https://www.nuget.org/packages/Automatron.AzureDevOps) enables you to write AzureDevOps workflows as .Net methods

Automatron.AzureDevOps also generates the AzureDevOps pipeline yaml evry time the project is compiled.

Steps can be are defined as void/async methods on one or many job classes

Inheritance and interfaces are supported as well

- [Quick start](#quick-start)
- [Variables](#variables)
- [Parameters](#parameters)

## Quick start

*The project needs to be in git repository to able to compute paths correctly or have the RootDir property set*

```c#
using System.Reflection;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;

return await new AzureDevOpsRunner().RunAsync(args);

[Pipeline("Ci")]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[Stage]
[Job]
public class Pipeline
{
    [Step(Emoji = "🏗"]
    public async Task Build()
    {
    }

    [Step(Emoji = "🧪", DependsOn = new[] { nameof(Build) })]
    public async Task Test()
    {
    }

    [Step(Emoji = "📦", DependsOn = new[] { nameof(Test) })]
    public async Task Pack()
    {
    }
}
```

Pipelines, Stage, Jobs, Steps can be run via cmd arguments  ```dotnet run -- Ci```

## Variables

Variables can mapped as properties

```c#
using System.Reflection;
using Automatron.AzureDevOps;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;

return await new AzureDevOpsRunner().RunAsync(args);

[Pipeline("Ci")]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("nuget")]
[Stage]
[Job]
public class Pipeline
{
    [Variable(Description = "The nuget api key")]
    public Secret? NugetApiKey { get; set; }

    [Step(Emoji = "🏗"]
    public async Task Build()
    {
    }

    [Step(Emoji = "🧪", DependsOn = new[] { nameof(Build) })]
    public async Task Test()
    {
    }

    [Step(Emoji = "📦", DependsOn = new[] { nameof(Test) })]
    public async Task Pack()
    {
    }
}
```

## Parameters
