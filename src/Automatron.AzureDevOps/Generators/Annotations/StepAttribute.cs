using System;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public abstract class StepAttribute : Attribute
    {
        public string? Job { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Condition { get; set; }

        public abstract Step Create(ISymbol symbol, IJob job);
    }
}