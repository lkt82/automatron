using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class JobVisitor : SymbolVisitor, IComparer<IJob>
{
    protected readonly Stage Stage;
 
    public List<IJob> Jobs { get; } = new();

    public JobVisitor(Stage stage)
    {
        Stage = stage;
    }

    public int Compare(IJob? x, IJob? y)
    {
        if (x == null || y == null)
        {
            return 0;
        }

        if (x.DependsOn != null && x.DependsOn.Contains(y.Name))
        {
            return 1;
        }

        if (y.DependsOn != null && y.DependsOn.Contains(x.Name))
        {
            return -1;
        }

        if (y.DependsOn != null && x.DependsOn == null)
        {
            return -1;
        }

        return 0;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var methods = symbol.GetAllPublicMethods();

        foreach (var method in methods)
        {
            method.Accept(this);
        }

        foreach (var job in Jobs)
        {
            var stepVisitor = new StepVisitor(job);
            symbol.Accept(stepVisitor);
        }

        Stage.Jobs.AddRange(Jobs);
        Stage.Jobs.Sort(this);
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        foreach (var attribute in symbol.GetCustomAbstractAttributes<JobAttribute>())
        {
            var stageName = !string.IsNullOrEmpty(attribute.Stage) ? attribute.Stage! : symbol.Name;

            if (stageName != Stage.Name)
            {
                continue;
            }

            if (attribute is DeploymentJobAttribute deploymentJobAttribute)
            {
                CreateDeploymentJob(deploymentJobAttribute, symbol);
            }
            else
            {
                CreateJob(attribute, symbol);
            }
        }
    }

    protected void CreateDeploymentJob(DeploymentJobAttribute jobAttribute, ISymbol member)
    {
        var job = CreateDeploymentJob(Stage, member, jobAttribute);

        Jobs.Add(job);
    }

    protected void CreateJob(JobAttribute jobAttribute, ISymbol member)
    {
        var job = CreateJob(Stage, member, jobAttribute);

        Jobs.Add(job);
    }

    private static DeploymentJob CreateDeploymentJob(Stage stage, ISymbol member, DeploymentJobAttribute jobAttribute)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

        var job = new DeploymentJob(stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition, jobAttribute.Environment)
        {
            TimeoutInMinutes = jobAttribute.TimeoutInMinutes == default
                ? null
                : jobAttribute.TimeoutInMinutes
        };

        if(!member.HasCustomAttributes<StageAttribute>())
        {
            var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

            if (poolAttribute != null)
            {
                job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }
        }

        if (!string.IsNullOrEmpty(stage.TemplateName))
        {
            job.TemplateName = stage.TemplateName;
        }

        return job;
    }

    private static Job CreateJob(Stage stage, ISymbol member, JobAttribute jobAttribute)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

        var job = new Job(stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition);

        if (!member.HasCustomAttributes<StageAttribute>())
        {
            var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

            if (poolAttribute != null)
            {
                job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }
        }

        if (!string.IsNullOrEmpty(stage.TemplateName))
        {
            job.TemplateName = stage.TemplateName;
        }

        return job;
    }
}