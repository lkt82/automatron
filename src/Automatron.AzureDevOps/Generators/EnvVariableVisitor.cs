using System.Collections.Generic;
using System.Text;
using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class EnvVariableVisitor : SymbolVisitor<IDictionary<string, object>>
{
    private readonly IJob _job;

    private ISymbol Type { get; set; }

    public EnvVariableVisitor(IJob job)
    {
        _job = job;
        Type = job.Symbol;
    }

    public override IDictionary<string, object>? VisitNamedType(INamedTypeSymbol symbol)
    {
        Type = symbol;

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
                    variables.Add(variable.Key, variable.Value);
                }

            }
        }

        Type = symbol;

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
                variables.Add(variable.Key, variable.Value);
            }
        }

        return variables;
    }

    public override IDictionary<string, object>? VisitProperty(IPropertySymbol symbol)
    {
        var variables = new Dictionary<string, object>();

        var variableAttribute = symbol.GetCustomAttribute<VariableAttribute>();

        if (variableAttribute != null)
        {

            var name = !string.IsNullOrEmpty(variableAttribute.Name) ? variableAttribute.Name : symbol.Name;
            var envName = GetEnvVarName(GetParameterName(name));

#pragma warning disable CS8603
#pragma warning disable CS8602
            variables.Add(envName, $"$({name})");
#pragma warning restore CS8602
#pragma warning restore CS8603
        }

        return variables;
    }

    private string GetParameterName(string name)
    {
        if (SymbolEqualityComparer.Default.Equals(Type, _job.Symbol))
        {
            var tokens = new List<string> { _job.Stage.Pipeline.Name };

            if (_job.Stage.Path != _job.Stage.Pipeline.Path)
            {
                tokens.Add(_job.Stage.Name);
            }

            if (_job.Path != _job.Stage.Path)
            {
                tokens.Add(_job.Name);
            }

            tokens.Add(name);

            return string.Join("-", tokens);
        }

        if (SymbolEqualityComparer.Default.Equals(Type, _job.Stage.Symbol))
        {
            var tokens = new List<string> { _job.Stage.Pipeline.Name };

            if (_job.Stage.Path != _job.Stage.Pipeline.Path)
            {
                tokens.Add(_job.Stage.Name);
            }

            tokens.Add(name);

            return string.Join("-", tokens);
        }

        if (SymbolEqualityComparer.Default.Equals(Type, _job.Stage.Pipeline.Symbol))
        {
            var tokens = new List<string> { _job.Stage.Pipeline.Name };

            tokens.Add(name);

            return string.Join("-", tokens);
        }

        return name;
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