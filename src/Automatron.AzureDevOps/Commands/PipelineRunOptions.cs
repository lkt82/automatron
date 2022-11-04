#if NET6_0
using Automatron.AzureDevOps.Models;
using CommandDotNet;

namespace Automatron.AzureDevOps.Commands;

public record PipelineRunOptions : IArgumentModel
{
    internal const string HiddenName = nameof(HiddenName);

    [Option('p', HiddenName)]
    public string? Pipeline { get; set; }

    [Option('s', "stage")]
    public string? Stage { get; set; }

    [Option('j', "job")]
    public string? Job { get; set; }

    [Option('t', "step")]
    public string? Step { get; set; }

    [Option('v', "variable")]
    public VariableValue[]? Variables { get; set; }
}
#endif