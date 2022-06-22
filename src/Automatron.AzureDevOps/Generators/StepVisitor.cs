using System.Collections.Generic;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class StepVisitor : SymbolVisitor
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

            CreateStep(attribute, symbol);
        }
    }

    private void CreateStep(StepAttribute stepAttribute, ISymbol member)
    {
        var step = stepAttribute.Create(member, _job);

        Steps.Add(step);
    }
}