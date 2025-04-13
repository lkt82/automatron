#if NET8_0
using CommandDotNet;

namespace Automatron.AzureDevOps.Commands;

public record PipelineRunArgs : IArgumentModel
{
    [Operand]
    public string? Pipeline { get; set; }
}
#endif