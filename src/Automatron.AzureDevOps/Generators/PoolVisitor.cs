using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class PoolVisitor : SymbolVisitor<Pool>
{
    public override Pool? VisitNamedType(INamedTypeSymbol symbol)
    {
        var poolAttribute = symbol.GetAllAttributes().GetCustomAttribute<PoolAttribute>();

        return poolAttribute != null ? CreatePool(poolAttribute) : null;
    }

    private static Pool CreatePool(PoolAttribute poolAttribute)
    {
        return new Pool(poolAttribute.Name, poolAttribute.VmImage);
    }
}