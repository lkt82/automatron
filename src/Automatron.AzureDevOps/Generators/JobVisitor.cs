#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Automatron.Collections;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class JobVisitor(Stage stage) : SymbolVisitor<IEnumerable<IJob>>
{
    private Dictionary<string, IJob> Jobs { get; } = new();

    public override IEnumerable<IJob> VisitNamedType(INamedTypeSymbol symbol)
    {
        VisitJobType(symbol);

        var list = Jobs.Values.TopologicalSort(x => x.DependsOn ?? Enumerable.Empty<string>(), x => x.Name);

        return list;
    }

    private void VisitJobType(INamedTypeSymbol symbol)
    {
        if (Jobs.ContainsKey(symbol.Name))
        {
            return;
        }

        var jobAttributes = symbol.GetAllCustomAttributes<JobAttribute>().ToArray();

        if (jobAttributes.Any())
        {
            var job = CreateJob(Merge(jobAttributes), symbol);

            Jobs[symbol.Name] = job;
        }

        foreach (var type in symbol.GetAllTypeMembers())
        {
            VisitJobType(type);
        }
    }

    private static JobAttribute Merge(IEnumerable<JobAttribute> jobAttributes)
    {
        var mergedJobAttribute = new JobAttribute();

        foreach (var jobAttribute in jobAttributes)
        {
            if (jobAttribute is DeploymentJobAttribute && mergedJobAttribute is not DeploymentJobAttribute)
            {
                mergedJobAttribute = new DeploymentJobAttribute
                {
                    Name = mergedJobAttribute.Name,
                    DisplayName = mergedJobAttribute.DisplayName,
                    DependsOn = mergedJobAttribute.DependsOn,
                    Condition = mergedJobAttribute.Condition
                };
            }

            mergedJobAttribute.Name = jobAttribute.Name ?? mergedJobAttribute.Name;
            mergedJobAttribute.DisplayName = jobAttribute.DisplayName ?? mergedJobAttribute.DisplayName;
            mergedJobAttribute.DependsOn = jobAttribute.DependsOn ?? mergedJobAttribute.DependsOn;
            mergedJobAttribute.Condition = jobAttribute.Condition ?? mergedJobAttribute.Condition;
            mergedJobAttribute.Emoji = jobAttribute.Emoji ?? mergedJobAttribute.Emoji;

            if (jobAttribute is DeploymentJobAttribute deploymentJobAttribute && mergedJobAttribute is DeploymentJobAttribute mergedDeploymentJobAttribute)
            {
                mergedDeploymentJobAttribute.Timeout = deploymentJobAttribute.Timeout ?? mergedDeploymentJobAttribute.Timeout;
                mergedDeploymentJobAttribute.Environment = deploymentJobAttribute.Environment ?? mergedDeploymentJobAttribute.Environment;
            }
        }

        return mergedJobAttribute;
    }

    private IJob CreateJob(JobAttribute jobAttribute, ISymbol symbol)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : symbol.Name;

        IJob? job;

        if (jobAttribute is DeploymentJobAttribute deploymentJobAttribute)
        {
            job =  new DeploymentJob(stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition, ParseEnvironment(deploymentJobAttribute.Environment) ?? throw new InvalidOperationException())
            {
                TimeoutInMinutes = deploymentJobAttribute.Timeout == null ? null : ParseTimeoutInMinutest(deploymentJobAttribute.Timeout)
            };
        }
        else
        {
            job = new Job(stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn, jobAttribute.Condition);
        }
     
        job.Pool = symbol.Accept(new PoolVisitor());
        job.Variables = symbol.Accept(new VariableVisitor());
        job.TemplateValues = symbol.Accept(new TemplateValueVisitor());
        job.Steps = symbol.Accept(new StepVisitor(job));

        return job;
    }

    private static int ParseTimeoutInMinutest(string environment)
    {
        return Convert.ToInt32(TimeSpan.Parse(environment).TotalMinutes);
    }

    private string? ParseEnvironment(string? environment)
    {
        if (string.IsNullOrEmpty(environment) || stage.TemplateParameters == null)
        {
            return environment;
        }

        var match = Regex.Match(environment, "^\\$\\{\\{(?<name>.+)\\}\\}");
        if (match.Success && stage.TemplateParameters.ContainsKey(match.Groups["name"].Value))
        {
            environment = (string)stage.TemplateParameters[match.Groups["name"].Value];
        }

        return environment;
    }

}
#endif