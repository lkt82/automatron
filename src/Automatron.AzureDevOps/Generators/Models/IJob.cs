#if NETSTANDARD2_0
using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models;

public interface IJob
{
    string Name { get; }

    string? DisplayName { get; }

    string[]? DependsOn { get; }

    string? Condition { get; }

    Pool? Pool { get; set; }

    IEnumerable<IVariable>? Variables { get; set; }

    IDictionary<string, object>? TemplateParameters { get; set; }

    IEnumerable<Step>? Steps { get; set; }

    Stage Stage { get; }
}
#endif