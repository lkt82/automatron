# Automatron.Tasks

[![NuGet version (Automatron.Tasks)](https://img.shields.io/nuget/v/Automatron.Tasks.svg?style=flat-square)](https://www.nuget.org/packages/Automatron.Tasks/)
[![Build status](https://dev.azure.com/lkt82/Public/_apis/build/status/Automatron%20CI?branchName=main)](https://dev.azure.com/lkt82/Public/_build/latest?definitionId=1)

The Automatron.Tasks [.NET library](https://www.nuget.org/packages/Automatron.Tasks) is a task automation system that enables you to write task based workflows as .Net methods

Tasks can be are defined as void/async methods on one or many classes

Inheritance and interfaces are supported as well

- [Quick start](#quick-start)
- [Dependencies](#dependencies)
- [Parameters](#parameters)

## Quick start

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

## Dependencies

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

### Parameters