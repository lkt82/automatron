using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class ConcreteTypeCollector : SymbolVisitor
{
    public List<INamedTypeSymbol> NamedTypes { get; } = new();

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var childSymbol in symbol.GetMembers())
        {
            childSymbol.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        if (symbol.IsStatic)
        {
            return;
        }

        if (symbol.TypeKind == TypeKind.Interface)
        {
            foreach (var nestedTypes in symbol.GetTypeMembers())
            {
                nestedTypes.Accept(this);
            }

            return;
        }

        if (symbol.TypeKind != TypeKind.Class || symbol.IsAbstract)
        {
            return;
        }

        NamedTypes.Add(symbol);
    }
}