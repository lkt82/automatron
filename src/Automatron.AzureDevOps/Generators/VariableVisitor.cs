using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
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

        //var properties = symbol.GetAllProperties();

        //foreach (var property in properties)
        //{
        //    var variables = property.Accept(this);

        //    if (variables == null)
        //    {
        //        continue;
        //    }

        //    foreach (var variable in variables)
        //    {
        //        yield return variable;
        //    }
        //}
    }

    //public override IEnumerable<IVariable>? VisitProperty(IPropertySymbol symbol)
    //{
    //    var variableAttribute = symbol.GetCustomAttribute<VariableAttribute>();

    //    if (variableAttribute != null)
    //    {
    //        yield return new Variable(variableAttribute.Name, variableAttribute.Value);
    //    }
    //}
}