#if NETSTANDARD2_0
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PipelineResourceVisitor : SymbolVisitor<IEnumerable<PipelineResource>>
{
    public override IEnumerable<PipelineResource>? VisitNamedType(INamedTypeSymbol symbol)
    {
        var pipelineTriggerAttributes = symbol.GetAllAttributes().GetCustomAttributes<PipelineTriggerAttribute>();

        foreach (var pipelineTriggerAttribute in pipelineTriggerAttributes)
        {
            yield return CreatePipelineResource(pipelineTriggerAttribute);
        }
    }

    private static PipelineResource CreatePipelineResource(PipelineTriggerAttribute pipelineTriggerAttribute)
    {
        var pipelineTrigger = new PipelineResource(pipelineTriggerAttribute.Name, pipelineTriggerAttribute.Source)
        {
            Project = pipelineTriggerAttribute.Project
        };

        CreatePipelineTrigger(pipelineTriggerAttribute, pipelineTrigger);

        return pipelineTrigger;
    }

    private static void CreatePipelineTrigger(PipelineTriggerAttribute pipelineTriggerAttribute,
        PipelineResource pipelineTrigger)
    {
        if (pipelineTriggerAttribute.IncludeBranches != null || pipelineTriggerAttribute.ExcludeBranches != null || pipelineTriggerAttribute.Stages != null || pipelineTriggerAttribute.Tags != null)
        {
            var trigger = new PipelineResourceTrigger
            {
                Tags = pipelineTriggerAttribute.Tags,
                Stages = pipelineTriggerAttribute.Stages
            };

            if (pipelineTriggerAttribute.IncludeBranches != null || pipelineTriggerAttribute.ExcludeBranches != null)
            {
                trigger.Branches = new TriggerBranches
                {
                    Include = pipelineTriggerAttribute.IncludeBranches,
                    Exclude = pipelineTriggerAttribute.ExcludeBranches
                };
            }

            pipelineTrigger.Trigger = trigger;
        }
        else
        {
            pipelineTrigger.Trigger = new AnyPipelineResourceTrigger();
        }
    }
}
#endif