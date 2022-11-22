using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Models;

public interface IJob
{
    string Name { get; }

    string? DisplayName { get; }

    string[]? DependsOn { get; }

    string? Condition { get; }

    Pool? Pool { get; }

    IEnumerable<IVariable>? Variables { get; set; }

    IDictionary<string, object>? TemplateParameters { get; set; }

    IEnumerable<Step>? Steps { get; set; }

    Stage Stage { get; }

    ISymbol Symbol { get; set; }
}