using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models;

public interface IJob
{
    string Name { get; }

    string? DisplayName { get; }

    string[]? DependsOn { get; }

    string? Condition { get; }

    Pool? Pool { get; }

    List<Step> Steps { get; }

    Stage Stage { get; }

    string? TemplateName { get; }
}