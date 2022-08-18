using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class VariableParameterVisitor : SymbolVisitor<IEnumerable<string>>
{
    public override IEnumerable<string>? VisitNamedType(INamedTypeSymbol symbol)
    {
        foreach (var constructor in symbol.Constructors)
        {
            foreach (var constructorParameter in constructor.Parameters)
            {
                var parameters = constructorParameter.Type.Accept(this);

                if (parameters == null)
                {
                    continue;
                }

                foreach (var parameter in parameters)
                {
                    yield return parameter;
                }

            }
        }

        var properties = symbol.GetAllProperties();

        foreach (var property in properties)
        {
            var parameters = property.Accept(this);

            if (parameters == null)
            {
                continue;
            }

            foreach (var parameter in parameters)
            {
                yield return parameter;
            }
        }
    }

    public override IEnumerable<string>? VisitProperty(IPropertySymbol symbol)
    {
        var variableAttribute = symbol.GetCustomAttribute<VariableAttribute>();

        if (variableAttribute != null)
        {
            yield return !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : symbol.Name;
        }
    }
}