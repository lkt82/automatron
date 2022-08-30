using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Models;

public interface IJob
{
    string Name { get; }

    string? DisplayName { get; }

    string[]? DependsOn { get; }

    string? Condition { get; }

    Pool? Pool { get; }

    IEnumerable<IVariable>? Variables { get; set; }

    IDictionary<string, object>? Parameters { get; set; }

    IEnumerable<Step>? Steps { get; set; }

    Stage Stage { get; }

    string Path { get; set; }

    ISymbol Symbol { get; set; }
}