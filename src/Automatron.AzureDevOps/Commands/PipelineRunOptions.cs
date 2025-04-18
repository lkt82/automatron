﻿#if NET8_0
using System.Collections.Generic;
using Automatron.AzureDevOps.Models;
using CommandDotNet;

namespace Automatron.AzureDevOps.Commands;

public record PipelineRunOptions : IArgumentModel
{
    [Option('s', "stage")]
    public string? Stage { get; set; }

    [Option('j', "job")]
    public string? Job { get; set; }

    [Option('t', "step")]
    public string? Step { get; set; }

    [Option('v', "variable")]
    public IEnumerable<VariableValue>? Variables { get; set; }

    [Option('p', "parameters")]
    public IEnumerable<ParameterValue>? Parameters { get; set; }

    [Option('n', "no-summary",BooleanMode = BooleanMode.Implicit )]
    public bool NoSummary { get; set; }

    [Option('d', "run-dependencies", BooleanMode = BooleanMode.Implicit)]
    public bool RunDependencies { get; set; }

    [Option('m', "dry-run", BooleanMode = BooleanMode.Implicit)]
    public bool DryRun { get; set; }
}
#endif