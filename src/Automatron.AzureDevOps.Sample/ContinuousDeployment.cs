﻿using Automatron.AzureDevOps.Annotations;
using Automatron.Models;

namespace Automatron.AzureDevOps.Sample;

[DeploymentJob("Deployment", Environment = "${{Environment}}")]
public abstract class PulumiDeploymentJob
{
    [Environment]
    public virtual string? Environment { get; set; }

    [Variable]
    public Secret? PulumiApiKey { get; set; }

    [Variable(Value = "AZURE_CLIENT_ID")]
    public virtual string? AzureClientId { get; set; }

    [Checkout(CheckoutSource.Self,FetchDepth = 0)]
    [NuGetAuthenticate]
    [Pulumi(DisplayName = "Pulumi install")]
    [Step]
    public virtual void Configure()
    {
        Console.WriteLine("Configure");
    }

    [Step(DependsOn = new[] { nameof(Configure) })]
    public virtual void Preview()
    {

    }

    [NuGetAuthenticate]
    [Step(DependsOn = new[] { nameof(Preview) })]
    public virtual void Update()
    {

    }
}

[Stage]
public abstract class PulumiDeploymentStage
{
    public class DeploymentJob : PulumiDeploymentJob
    {
        [Step(DependsOn = new[] { nameof(Configure) })]
        public virtual void AfterConfigure()
        {

        }

        [Step(DependsOn = new[] { nameof(AfterConfigure) })]
        public override void Preview()
        {
            base.Preview();
        }
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