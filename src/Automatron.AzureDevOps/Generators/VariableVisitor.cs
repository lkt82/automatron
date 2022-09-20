using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class VariableVisitor : SymbolVisitor<IEnumerable<IVariable>>
{
    public override IEnumerable<IVariable>? VisitNamedType(INamedTypeSymbol symbol)
    {
        var attributes = symbol.GetAllAttributes(); 
        
        var variableGroupAttributes = symbol.GetCustomAttributes<VariableGroupAttribute>();
        var variableAttributes = attributes.GetCustomAttributes<VariableAttribute>();

        foreach (var variableGroupAttribute in variableGroupAttributes)
        {
            yield return new VariableGroup(variableGroupAttribute.Name);
        }

        foreach (var variableAttribute in variableAttributes)
        {
            if(!string.IsNullOrEmpty(variableAttribute.Name) && variableAttribute.Value != null)
            {
                #pragma warning disable CS8604
                yield return new Variable(variableAttribute.Name, variableAttribute.Value);
                #pragma warning restore CS8604
            }
        }
    }
}