using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class CiTriggerVisitor : SymbolVisitor<ICiTrigger>
{
    public override ICiTrigger? VisitNamedType(INamedTypeSymbol symbol)
    {
        var ciTriggerAttribute = symbol.GetAllAttributes().GetCustomAttribute<CiTriggerAttribute>();

        return ciTriggerAttribute != null ? CreateCiTrigger(ciTriggerAttribute) : null;
    }

    private static ICiTrigger CreateCiTrigger(CiTriggerAttribute ciTriggerAttribute)
    {
        if (ciTriggerAttribute.Disabled)
        {
            return new DisabledCiTrigger();
        }

        var trigger = new CiTrigger
        {
            Batch = ciTriggerAttribute.Batch
        };

        if (ciTriggerAttribute.IncludeBranches != null || ciTriggerAttribute.ExcludeBranches != null)
        {
            trigger.Branches = new TriggerBranches
            {
                Include = ciTriggerAttribute.IncludeBranches,
                Exclude = ciTriggerAttribute.ExcludeBranches
            };
        }

        if (ciTriggerAttribute.IncludePaths != null || ciTriggerAttribute.ExcludePaths != null)
        {
            trigger.Paths = new TriggerPaths
            {
                Include = ciTriggerAttribute.IncludePaths,
                Exclude = ciTriggerAttribute.ExcludePaths
            };
        }
        return trigger;
    }
}