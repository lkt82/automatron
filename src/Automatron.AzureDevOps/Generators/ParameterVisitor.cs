#if NETSTANDARD2_0
using System.Collections.Generic;
using Automatron.AzureDevOps.Annotations;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class ParameterVisitor : SymbolVisitor<IEnumerable<Models.Parameter>>
{
    public override IEnumerable<Models.Parameter>? VisitNamedType(INamedTypeSymbol symbol)
    {
        var parameterAttributes = symbol.GetAllAttributes().GetCustomAttributes<ParameterAttribute>();

        foreach (var parameterAttribute in parameterAttributes)
        {
            var parameter = CreatePipelineParameter(parameterAttribute);

            if (parameter == null)
            {
                continue;
            }

            yield return parameter;
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

    public override IEnumerable<Models.Parameter>? VisitProperty(IPropertySymbol symbol)
    {
        var parameterAttribute = symbol.GetCustomAttribute<ParameterAttribute>();

        if (parameterAttribute != null)
        {
            var name = !string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name : symbol.Name;

            var parameter = CreatePipelineParameter(name, parameterAttribute);

            if (parameter != null)
            {
                yield return parameter;
            }
        }
    }

    private static Models.Parameter? CreatePipelineParameter(ParameterAttribute parameterAttribute)
    {
        return CreatePipelineParameter(parameterAttribute.Name, parameterAttribute);
    }

    private static Models.Parameter? CreatePipelineParameter(string? name, ParameterAttribute parameterAttribute)
    {
        string? type = null;

        if (parameterAttribute.Value != null)
        {
            type = GetParameterType(parameterAttribute.Value);
        }
        else if (parameterAttribute.Values is { Length: > 0 })
        {
            type = GetParameterType(parameterAttribute.Values[1]);
        }

        if (string.IsNullOrEmpty(name) && type == null)
        {
            return null;
        }

        #pragma warning disable CS8604
        return new Models.Parameter(name, parameterAttribute.DisplayName, type, parameterAttribute.Value, parameterAttribute.Values);
        #pragma warning restore CS8604
    }


    private static string? GetParameterType(object type)
    {
        return type switch
        {
            string => "string",
            bool => "boolean",
            int => "number",
            double => "number",
            decimal => "number",
            _ => null
        };
    }
}
#endif