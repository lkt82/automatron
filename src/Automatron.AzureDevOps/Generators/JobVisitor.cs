using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class JobVisitor : SymbolVisitor
{
    private readonly Stage _stage;
 
    public List<IJob> Jobs { get; } = new();

    public JobVisitor(Stage stage)
    {
        _stage = stage;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var publicMembers = symbol.GetAllPublicMethods();

        foreach (var member in publicMembers)
        {
            member.Accept(this);
        }

        foreach (var job in Jobs)
        {
            var stepVisitor = new StepVisitor(job);
            symbol.Accept(stepVisitor);

            job.Steps.AddRange(stepVisitor.Steps);
        }
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        foreach (var attribute in symbol.GetCustomAttributes<JobAttribute>())
        {
            var stageName = !string.IsNullOrEmpty(attribute.Stage) ? attribute.Stage! : symbol.Name;

            if (stageName != _stage.Name && string.IsNullOrEmpty(_stage.Template))
            {
                continue;
            }

            CreateJob(attribute, symbol);
        }

        foreach (var attribute in symbol.GetCustomAttributes<DeploymentJobAttribute>())
        {
            var stageName = !string.IsNullOrEmpty(attribute.Stage) ? attribute.Stage! : symbol.Name;

            if (stageName != _stage.Name && string.IsNullOrEmpty(_stage.Template))
            {
                continue;
            }

            CreateDeploymentJob(attribute, symbol);
        }

        foreach (var stageAttribute in symbol.GetCustomAttributes<StageAttribute>())
        {
            if (stageAttribute.Pipeline != _stage.Pipeline.Name && stageAttribute.Pipeline != null)
            {
                continue;
            }

            stageAttribute.TemplateSymbol?.Accept(this);
        }
    }

    private void CreateDeploymentJob(DeploymentJobAttribute jobAttribute, ISymbol member)
    {
        var job = CreateDeploymentJob(_stage, member, jobAttribute);

        Jobs.Add(job);
    }

    private void CreateJob(JobAttribute jobAttribute, ISymbol member)
    {
        var job = CreateJob(_stage, member, jobAttribute);

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
        //if (job.Stage.Name != name)
        {
            var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

            if (poolAttribute != null)
            {
                job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }
        }

        if (!string.IsNullOrEmpty(stage.Template))
        {
            job.Template = stage.Template;
        }

        return job;
    }

    private static Job CreateJob(Stage stage, ISymbol member, JobAttribute jobAttribute)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : member.Name;

        var job = new Job(stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition);

        if (job.Stage.Name != name)
        {
            var poolAttribute = member.GetCustomAttributes<PoolAttribute>().FirstOrDefault(c => c.Target == name || c.Target == null);

            if (poolAttribute != null)
            {
                job.Pool = new Pool(poolAttribute.Name, poolAttribute.VmImage);
            }
        }

        if (!string.IsNullOrEmpty(stage.Template))
        {
            job.Template = stage.Template;
        }

        return job;
    }
}