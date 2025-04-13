#if NET8_0
using System.Collections.Generic;
using Automatron.Tasks.Models;
using CommandDotNet;

namespace Automatron.Tasks.Commands;

public record TaskRunOptions : IArgumentModel
{
    [Option('p', Description = "Parameters to use", Split = ',')]
    public IEnumerable<ParameterValue>? Parameters { get; set; }

    [Option('s', Description = "Dependencies to skip", Split = ',')]
    public IEnumerable<string>? Skip { get; set; }
}
#endif