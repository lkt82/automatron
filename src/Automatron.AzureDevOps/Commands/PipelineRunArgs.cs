#if NET6_0
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Models;
using CommandDotNet;

namespace Automatron.AzureDevOps.Commands;

public record PipelineRunArgs : IArgumentModel
{
    [Operand]
    public string? Pipeline { get; set; }

    [Operand] public IEnumerable<ParameterValue> Parameters { get; set; } = Enumerable.Empty<ParameterValue>();
}
#endif