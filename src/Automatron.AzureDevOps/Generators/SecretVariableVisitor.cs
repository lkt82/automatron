using System.Collections.Generic;
using Automatron.AzureDevOps.CodeAnalysis;
using Automatron.AzureDevOps.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators;

internal class SecretVariableVisitor : SymbolVisitor
{
    private readonly Pipeline _pipeline;

    public List<string> Secrets { get; } = new();

    public SecretVariableVisitor(Pipeline pipeline)
    {
        _pipeline = pipeline;
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var properties = symbol.GetAllProperties();

        foreach (var property in properties)
        {
            property.Accept(this);
        }
        _pipeline.Secrets.AddRange(Secrets);
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        //var secretVariableAttributes = symbol.GetCustomAttributes<SecretVariableAttribute>().Where(c => c.Pipeline == _pipeline.Name || c.Pipeline == null);

        //foreach (var secretVariableAttribute in secretVariableAttributes)
        //{
        //    Secrets.Add(!string.IsNullOrEmpty(secretVariableAttribute.Name) ? secretVariableAttribute.Name! : symbol.Name);
        //}
    }
}