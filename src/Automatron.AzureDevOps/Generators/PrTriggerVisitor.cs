#if NETSTANDARD2_0
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PrTriggerVisitor : SymbolVisitor<IPrTrigger>
{
    public override IPrTrigger? VisitNamedType(INamedTypeSymbol symbol)
    {
        var prTriggerAttribute = symbol.GetAllAttributes().GetCustomAttribute<PrTriggerAttribute>();

        return prTriggerAttribute != null ? CreatePrTrigger(prTriggerAttribute) : null;
    }

    private static IPrTrigger CreatePrTrigger(PrTriggerAttribute prTriggerAttribute)
    {
        if (prTriggerAttribute.Disabled)
        {
            return new DisabledPrTrigger();
        }

        var trigger = new PrTrigger
        {
            AutoCancel = prTriggerAttribute.AutoCancel == false ? false : null,
            Drafts = prTriggerAttribute.Drafts == false ? false : null
        };

        if (prTriggerAttribute.IncludeBranches != null || prTriggerAttribute.ExcludeBranches != null)
        {
            trigger.Branches = new TriggerBranches
            {
                Include = prTriggerAttribute.IncludeBranches,
                Exclude = prTriggerAttribute.ExcludeBranches
            };
        }

        if (prTriggerAttribute.IncludePaths != null || prTriggerAttribute.ExcludePaths != null)
        {
            trigger.Paths = new TriggerPaths
            {
                Include = prTriggerAttribute.IncludePaths,
                Exclude = prTriggerAttribute.ExcludePaths
            };
        }
        return trigger;
    }
}
#endif