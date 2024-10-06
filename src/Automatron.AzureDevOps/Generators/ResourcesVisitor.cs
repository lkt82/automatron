#if NETSTANDARD2_0
using System.Linq;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class ResourcesVisitor : SymbolVisitor<Resources>
{
    public override Resources? VisitNamedType(INamedTypeSymbol symbol)
    {
        var pipelines = symbol.Accept(new PipelineResourceVisitor());

        if (pipelines == null || !pipelines.Any())
        {
            return null;
        }

        var resources = new Resources
        {
            Pipelines = symbol.Accept(new PipelineResourceVisitor())
        };

        return resources;
    }
}

#endif