# Automatron
[![Build status](https://dev.azure.com/lkt82/Public/_apis/build/status/Automatron%20CI?branchName=main)](https://dev.azure.com/lkt82/Public/_build/latest?definitionId=1)

The Automatron Platform is a collection of .NET libraries to help .Net Developers create automation workloads in the in the same language as your .Net applications

Platform support: [.NET 6.0 and later](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-6).

- [Tasks](#tasks)
    - [Quick start](#tasks-quick-start)
    - [Dependencies](#tasks-dependencies)
    - [Parameters](#tasks-parameters)
- [AzureDevOps](#azuredevops)
  - [Quick start](#azuredevops-quick-start)
  - [Variables](#azuredevops-variables)
  - [Parameters](#azuredevops-parameters)

## Tasks

[![NuGet version (Automatron.Tasks)](https://img.shields.io/nuget/v/Automatron.Tasks.svg?style=flat-square)](https://www.nuget.org/packages/Automatron.Tasks/)

The Automatron.Tasks [.NET library](https://www.nuget.org/packages/Automatron.Tasks) is a task automation system that enables you to write task based workflows as .Net methods

Tasks can be are defined as void/async methods on one or many classes

Inheritance and interfaces are supported as well

### Tasks Quick start

- Create a .NET console app named `Sample` and add a reference to [Automatron.Tasks](https://www.nuget.org/packages/Automatron.Tasks).
- Replace the contents in `Program.cs` with:
```c#
using Automatron.Tasks;
using Automatron.Tasks.Annotations;

return await new TaskRunner().RunAsync(args);

namespace Sample;

public class Build
{
    [Task(Default = true)]
    public void Default()
    {
        Console.WriteLine("Hello, world!");
    }

    [Task]
    public void Build()
    {
        Console.WriteLine("Building my app");
    }
}
```
- Run the app. E.g. `dotnet run` or F5 in Visual Studio:

Tasks can be run via cmd arguments  ```dotnet run -- Build```

### Tasks Dependencies

Dependencies are expression via the attributes

- DependentOn 
- DependentFor

```c#
using Automatron.Tasks;
using Automatron.Tasks.Annotations;

return await new TaskRunner().RunAsync(args);

namespace Sample;

public class Build
{
    [Task(Default = true)]
    [DependentOn(nameof(Build))]
    public void Default()
    {
        Console.WriteLine("Hello, world!");
    }

    [Task]
    public void Build()
    {
        Console.WriteLine("Building my app");
    }
}
```

### Tasks Parameters

## AzureDevOps

[![NuGet version (Automatron.AzureDevOps)](https://img.shields.io/nuget/v/Automatron.AzureDevOps.svg?style=flat-square)](https://www.nuget.org/packages/Automatron.AzureDevOps/)

The Automatron.AzureDevOps [.NET library](https://www.nuget.org/packages/Automatron.AzureDevOps) enables you to write AzureDevOps workflows as .Net methods

Automatron.AzureDevOps also generates the AzureDevOps pipeline yaml evry time the project is compiled.

Steps can be are defined as void/async methods on one or many job classes

Inheritance and interfaces are supported as well

### AzureDevOps Quick start

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

### AzureDevOps Variables

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

### AzureDevOps Parameters
