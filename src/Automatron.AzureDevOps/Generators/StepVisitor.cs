using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StepVisitor : SymbolVisitor, IComparer<Step>
{
    private readonly IJob _job;
    public List<Step> Steps { get; } = new();

    public StepVisitor(IJob job)
    {
        _job = job;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var methods = symbol.GetAllPublicMethods();

        foreach (var method in methods)
        {
            method.Accept(this);
        }

        _job.Steps.AddRange(Steps);
        _job.Steps.Sort(this);
    }

    public override void VisitMethod(IMethodSymbol symbol)
    {
        foreach (var attribute in symbol.GetCustomAbstractAttributes<StepAttribute>())
        {
            var jobName = !string.IsNullOrEmpty(attribute.Job) ? attribute.Job! : symbol.Name;

            if (jobName != _job.Name && string.IsNullOrEmpty(_job.TemplateName))
            {
                continue;
            }

            var step = CreateStep(attribute, symbol);

            Steps.Add(step);
        }
    }

    private Step CreateStep(StepAttribute stepAttribute, ISymbol member)
    {
        var step = stepAttribute.Create(member, _job);

        return step;
    }

    public int Compare(Step? x, Step? y)
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
}