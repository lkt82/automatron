using System;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StageAttribute: Attribute
    {
        public string? Pipeline { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public StageAttribute()
        {
        }

        public StageAttribute(string pipeline)
        {
            Pipeline = pipeline;
        }

        public Type? Template { get; }

        public StageAttribute(Type template)
        {
            Template = template;
        }
        public StageAttribute(string pipeline, Type template) : this(pipeline)
        {
            Template = template;
        }

        internal INamedTypeSymbol? TemplateSymbol { get; }

        internal StageAttribute(INamedTypeSymbol templateSymbol)
        {
            TemplateSymbol = templateSymbol;
        }

        internal StageAttribute(string pipeline, INamedTypeSymbol templateSymbol) : this(pipeline)
        {
            TemplateSymbol = templateSymbol;
        }
    }
}
