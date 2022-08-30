using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class JobVisitor : SymbolVisitor<IEnumerable<IJob>>, IComparer<IJob>
{
    private readonly Stage _stage;

    private Dictionary<string, IJob> Jobs { get; } = new();

    private INamedTypeSymbol? _root;

    public JobVisitor(Stage stage)
    {
        _stage = stage;
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

    public override IEnumerable<IJob> VisitNamedType(INamedTypeSymbol symbol)
    {
        _root = symbol;

        VisitJobType(symbol);

        var list = new List<IJob>(Jobs.Values);
        list.Sort(this);

        return list;
    }

    private void VisitJobType(INamedTypeSymbol symbol)
    {
        if (Jobs.ContainsKey(symbol.Name))
        {
            return;
        }

        var jobAttribute = symbol.GetAllAttributes().GetCustomAbstractAttribute<JobAttribute>();

        if (jobAttribute != null)
        {
            if (jobAttribute.DependsOn != null)
            {
                foreach (var jobTypeSymbol in jobAttribute.DependsOn.Cast<INamedTypeSymbol>())
                {
                    VisitJobType(jobTypeSymbol);
                }
            }

            IJob job;

            if (jobAttribute is DeploymentJobAttribute deploymentJobAttribute)
            {
                job = CreateDeploymentJob(deploymentJobAttribute, symbol);
            }
            else
            {
                job = CreateJob(jobAttribute, symbol);
            }
         
            job.Parameters = symbol.Accept(new TemplateParameterVisitor());

            if (SymbolEqualityComparer.Default.Equals(_root, symbol))
            {
                job.Path = _stage.Path;
            }
            else
            {
                job.Path = _stage.Path + "/" + job.Name;
            }

            job.Steps = symbol.Accept(new StepVisitor(job));

            Jobs[symbol.Name] = job;
        }

        foreach (var type in symbol.GetAllTypeMembers())
        {
            VisitJobType(type);
        }
    }

    private Job CreateJob(JobAttribute jobAttribute, ISymbol symbol)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : symbol.Name;

        var job = new Job(_stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn?.Cast<ISymbol>().Select(c => Jobs[c.Name].Name).ToArray(), jobAttribute.Condition, symbol)
            {
                Pool = symbol.Accept(new PoolVisitor()),
                Variables = symbol.Accept(new VariableVisitor())
            };

        return job;
    }

    private DeploymentJob CreateDeploymentJob(DeploymentJobAttribute jobAttribute, ISymbol symbol)
    {
        var name = !string.IsNullOrEmpty(jobAttribute.Name) ? jobAttribute.Name! : symbol.Name;

        var environment = jobAttribute.Environment;

        if (!string.IsNullOrEmpty(environment) && _stage.TemplateParameters != null)
        {
            var match = Regex.Match(environment, "^\\$\\{\\{(?<name>.+)\\}\\}");
            if (match.Success)
            {
                environment = (string)_stage.TemplateParameters[match.Groups["name"].Value];
            }
        }

        var job = new DeploymentJob(_stage, name, jobAttribute.DisplayName, jobAttribute.DependsOn?.Cast<ISymbol>().Select(c => Jobs[c.Name].Name).ToArray(), jobAttribute.Condition, environment ?? throw new InvalidOperationException(),symbol)
        {
            TimeoutInMinutes = jobAttribute.Timeout == null ? null : Convert.ToInt32(TimeSpan.Parse(jobAttribute.Timeout).TotalMinutes),
            Pool = symbol.Accept(new PoolVisitor()),
            Variables = symbol.Accept(new VariableVisitor())
        };


        return job;
    }
}