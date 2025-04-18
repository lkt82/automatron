﻿#if NETSTANDARD2_0
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class TemplateValueVisitor : SymbolVisitor<IDictionary<string, object>>
{
    public override IDictionary<string, object> VisitNamedType(INamedTypeSymbol symbol)
    {
        var parameters = new Dictionary<string, object>();

        var parameterAttributes = symbol.GetAllCustomAttributes<TemplateParameterAttribute>();
        foreach (var variableAttribute in parameterAttributes)
        {
            if (variableAttribute.Name == null || variableAttribute.Value == null)
            {
                continue;
            }
            parameters.Add(variableAttribute.Name, variableAttribute.Value);
        }

        return parameters;
    }

}
#endif