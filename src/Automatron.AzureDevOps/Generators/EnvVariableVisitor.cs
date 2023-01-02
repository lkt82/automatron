using System.Collections.Generic;
using System.Text;
using Automatron.AzureDevOps.Annotations;
using Automatron.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class EnvVariableVisitor : SymbolVisitor<Dictionary<string, object>>
{
    public override Dictionary<string, object>? VisitNamedType(INamedTypeSymbol symbol)
    {
        var variables = new Dictionary<string, object>();

        foreach (var constructor in symbol.Constructors)
        {
            foreach (var constructorParameter in constructor.Parameters)
            {
                var constructorVariables = constructorParameter.Type.Accept(this);

                if (constructorVariables == null)
                {
                    continue;
                }

                foreach (var variable in constructorVariables)
                {
                    if (variables.ContainsKey(variable.Key))
                    {
                        continue;
                    }
                    variables.Add(variable.Key, variable.Value);
                }

            }
        }

        var properties = symbol.GetAllProperties();

        foreach (var property in properties)
        {
            var propertyVariables = property.Accept(this);

            if (propertyVariables == null)
            {
                continue;
            }

            foreach (var variable in propertyVariables)
            {
                if (variables.ContainsKey(variable.Key))
                {
                    continue;
                }
                variables.Add(variable.Key, variable.Value);
            }
        }

        return variables.Count == 0 ? null : variables;
    }

    public override Dictionary<string, object> VisitProperty(IPropertySymbol symbol)
    {
        var variables = new Dictionary<string, object>();

        var variableAttribute = symbol.GetCustomAttribute<VariableAttribute>();

        if (variableAttribute != null)
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            var name = (!string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : symbol.Name)!;
            var envName = GetEnvVarName(name);
            variables.Add(envName, variableAttribute.Value != null ? $"$({variableAttribute.Value})" : $"$({name})");
        }

        var templateParameterAttribute = symbol.GetCustomAttribute<ParameterAttribute>();

        if (templateParameterAttribute != null)
        {
            // ReSharper disable once RedundantSuppressNullableWarningExpression
            var name = (!string.IsNullOrEmpty(templateParameterAttribute.Name) ? templateParameterAttribute.Name : symbol.Name)!;
            var envName = GetEnvVarName(name);
            variables.Add(envName, $"${{{{ parameters.{name} }}}}");
        }

        return variables;
    }

    private static string GetEnvVarName(string name)
    {
        name = name.Replace("-","_");

        var envVarName = new StringBuilder();

        for (var index = 0; index < name.Length; index++)
        {
            var n = name[index];
            if (index > 0 && char.IsLower(name[index - 1]) && char.IsUpper(n))
            {
                envVarName.Append('_');
                envVarName.Append(n);
            }
            else if (char.IsLower(n))
            {
                envVarName.Append(char.ToUpper(n));
            }
            else
            {
                envVarName.Append(n);
            }
        }

        return envVarName.ToString();
    }
}