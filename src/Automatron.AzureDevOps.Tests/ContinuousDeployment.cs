﻿using Automatron.AzureDevOps.Annotations;

namespace Automatron.AzureDevOps.Tests;

public abstract class PulumiDeploymentJob
{
    [Environment]
    public virtual string? Environment { get; set; }

    [Step]
    public virtual void Init()
    {

    }

    [Step(DependsOn = new[] { nameof(Init) })]
    public virtual void Preview()
    {

    }

    [Step(DependsOn = new[] { nameof(Preview) })]
    public virtual void Update()
    {

    }
}

[Pool(VmImage = "ubuntu-latest")]
public abstract class PulumiDeploymentStage
{
    [DeploymentJob]
    public class DeploymentJob : PulumiDeploymentJob
    {

    }
}

[Pipeline("Ci")]
[CiTrigger(Batch = true, IncludeBranches = new[] { "main" }, IncludePaths = new[] { "src" })]
[Pool(VmImage = "ubuntu-latest")]
[VariableGroup("Nuget")]
public abstract class PulumiContinuousDeploymentPipeline
{
    [Stage]
    [Environment("Testing")]
    public class DeployToTesting : PulumiDeploymentStage
    {
    }

    [Stage(DependsOn = new [] { nameof(DeployToTesting) })]
    [Environment("Staging")]
    public class DeployToStaging : PulumiDeploymentStage
    {
    }

    [Stage(DependsOn = new [] { nameof(DeployToStaging) })]
    [Environment("Production")]
    public class DeployToProduction : PulumiDeploymentStage
    {
    }
}

public class ContinuousDeployment : PulumiContinuousDeploymentPipeline
{
}