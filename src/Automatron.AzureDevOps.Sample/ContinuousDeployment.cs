using Automatron.AzureDevOps.Annotations;
using Automatron.Models;

namespace Automatron.AzureDevOps.Sample;

[DeploymentJob("Deployment", Environment = "${{Environment}}")]
public abstract class PulumiDeploymentJob
{
    [Environment]
    public virtual string? Environment { get; set; }

    [Variable]
    public Secret? PulumiApiKey { get; set; }

    [Step]
    [Checkout(CheckoutSource.Self,FetchDepth = 0)]
    [NuGetAuthenticate]
    [Pulumi(DisplayName = "Pulumi install")]
    public virtual void Init()
    {
       //throw new Exception(":/:/:/");
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

[Stage]
public abstract class PulumiDeploymentStage
{
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
    [Variable]
    public Secret? PulumiApiKey { get; set; }

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