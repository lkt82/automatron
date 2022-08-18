using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class ScheduledTriggerVisitor : SymbolVisitor<IEnumerable<ScheduledTrigger>>
{
    public override IEnumerable<ScheduledTrigger>? VisitNamedType(INamedTypeSymbol symbol)
    {
        var scheduledTriggerAttributes = symbol.GetAllAttributes().GetCustomAttributes<ScheduledTriggerAttribute>();

        foreach (var scheduledTriggerAttribute in scheduledTriggerAttributes)
        {
            yield return CreateScheduledTrigger(scheduledTriggerAttribute);
        }
    }

    private static ScheduledTrigger CreateScheduledTrigger(ScheduledTriggerAttribute scheduledTriggerAttribute)
    {
        var scheduledTrigger = new ScheduledTrigger(scheduledTriggerAttribute.Cron)
        {
            Always = scheduledTriggerAttribute.Always,
            DisplayName = scheduledTriggerAttribute.DisplayName
        };

        if (scheduledTriggerAttribute.IncludeBranches != null || scheduledTriggerAttribute.ExcludeBranches != null)
        {
            scheduledTrigger.Branches = new TriggerBranches
            {
                Include = scheduledTriggerAttribute.IncludeBranches,
                Exclude = scheduledTriggerAttribute.ExcludeBranches
            };
        }
        return scheduledTrigger;
    }
}