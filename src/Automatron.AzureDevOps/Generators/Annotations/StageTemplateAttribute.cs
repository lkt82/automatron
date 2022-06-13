using System;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class StageTemplateAttribute : StageAttribute
{
    public Type? TemplateType { get; }

    public StageTemplateAttribute(Type templateType)
    {
        TemplateType = templateType;
    }
    public StageTemplateAttribute(string pipeline, Type templateType) : base(pipeline)
    {
        TemplateType = templateType;
    }

    internal INamedTypeSymbol? TemplateTypeSymbol { get; }

    internal StageTemplateAttribute(INamedTypeSymbol typeSymbol)
    {
        TemplateTypeSymbol = typeSymbol;
    }

    internal StageTemplateAttribute(string pipeline, INamedTypeSymbol typeSymbol) : base(pipeline)
    {
        TemplateTypeSymbol = typeSymbol;
    }

}