# Automatron.GitHubActions

[![NuGet version (Automatron.GitHub)](https://img.shields.io/nuget/v/Automatron.GitHub.svg?style=flat-square)](https://www.nuget.org/packages/Automatron.GitHub/)

The Automatron.GitHub [.NET library](https://www.nuget.org/packages/Automatron.GitHub) enables you to write GitHub Actions workflows as .Net code

Automatron.GitHub also generates the GitHub Actions workflow YAML every time the project is compiled.

Steps can be defined as void/async methods on one or many job classes

Inheritance and interfaces are supported as well

## Quick start

*The project needs to be in git repository to able to compute paths correctly or have the RootDir property set*

```c#
using System.Reflection;
using Automatron.GitHub;
using Automatron.GitHub.Annotations;
using Automatron.GitHub.Tasks;

return await new GitHubActionsRunner().RunAsync(args);

[Workflow("CI")]
[Push(Branches = ["main"], Paths = ["src/**"])]
[PullRequest(Branches = ["main"])]
[RunsOn("ubuntu-latest")]
[Job]
public class Pipeline
{
    [Step(Name = "Build", Uses = "actions/setup-dotnet@v4")]
    public async Task Build()
    {
    }

    [Step(Name = "Test", Needs = ["Build"])]
    public async Task Test()
    {
    }

    [Step(Name = "Deploy", Needs = ["Test"])]
    public async Task Deploy()
    {
    }
}
```

Workflows, Jobs, Steps can be run via cmd arguments  ```dotnet run -- CI```

## Variables

Variables can mapped as properties

```c#
using System.Reflection;
using Automatron.GitHubActions;
using Automatron.GitHubActions.Annotations;
using Automatron.GitHubActions.Tasks;

return await new GitHubActionsRunner().RunAsync(args);

[Workflow("CI")]
[Push(Branches = ["main"], Paths = ["src/**"])]
[PullRequest(Branches = ["main"])]
[RunsOn("ubuntu-latest")]
[Job]
public class Pipeline
{
    [Variable(Description = "The deployment environment")]
    public string? Environment { get; set; }

    [Secret(Description = "The API key for deployment")]
    public Secret? ApiKey { get; set; }

    [Step(Name = "Build")]
    public async Task Build()
    {
    }

    [Step(Name = "Test", Needs = ["Build"])]
    public async Task Test()
    {
    }

    [Step(Name = "Deploy", Needs = ["Test"], If = "${{ env.Environment == 'production' }}")]
    public async Task Deploy()
    {
    }
}
```

## Parameters

Parameters can be defined for workflow inputs:

```c#
[Workflow("Deploy")]
[WorkflowDispatch]
[Job]
public class DeploymentPipeline
{
    [Input(Description = "Target environment", Required = true, Default = "staging")]
    public string Environment { get; set; } = "staging";

    [Input(Description = "Version to deploy", Required = true)]
    public string Version { get; set; } = "";

    [Step(Name = "Deploy to Environment")]
    public async Task Deploy()
    {
        Console.WriteLine($"Deploying version {Version} to {Environment}");
    }
}
```