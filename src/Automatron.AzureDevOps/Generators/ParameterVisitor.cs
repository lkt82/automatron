﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Generators.Annotations;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class ParameterVisitor : SymbolVisitor
{
    private readonly Pipeline _pipeline;

    public List<Models.Parameter> Parameters { get; } = new();

    public ParameterVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var properties = symbol.GetAllPublicProperties();

        foreach (var property in properties)
        {
            property.Accept(this);
        }
        _pipeline.Parameters.AddRange(Parameters);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        //var parameterAttributes = symbol.GetCustomAttributes<ParameterAttribute>().Where(c => c.Pipeline == _pipeline.Name || c.Pipeline == null);

        //foreach (var parameterAttribute in parameterAttributes)
        //{
        //    Parameters.Add(new Parameter(!string.IsNullOrEmpty(parameterAttribute.Name) ? parameterAttribute.Name! : symbol.Name, parameterAttribute.DisplayName, !string.IsNullOrEmpty(parameterAttribute.Type) ? parameterAttribute.Type! : GetParameterType(symbol.Type), parameterAttribute.Default, parameterAttribute.Values));
        //}
    }

    private static string GetParameterType(ITypeSymbol type)
    {
        switch (type.Name)
        {
            case "String":
                return ParameterTypes.String;
            case "Boolean":
                return ParameterTypes.Boolean;
            case "Int":
                return ParameterTypes.Number;
            default:
                throw new NotSupportedException();

        }

    }
}