using System.Collections.Generic;

namespace Automatron.AzureDevOps.Models;

public interface IJob
{
    string Name { get; }

    string? DisplayName { get; }

    string[]? DependsOn { get; }

    string? Condition { get; }

    Pool? Pool { get; }

    IEnumerable<IVariable>? Variables { get; set; }

    IEnumerable<string>? Parameters { get; set; }

    IEnumerable<Step>? Steps { get; set; }

    Stage Stage { get; }

    string Path { get; set; }
}