#if NET6_0
using Automatron.AzureDevOps.Models;
using CommandDotNet;

namespace Automatron.AzureDevOps.Commands;

public record PipelineRunArgs : IArgumentModel
{
    [Operand]
    public string Pipeline { get; set; } = null!;

    [Operand]
    public ParameterValue[]? Parameters { get; set; }
}
#endif