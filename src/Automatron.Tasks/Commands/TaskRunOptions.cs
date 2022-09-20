#if NET6_0
using System.Collections.Generic;
using CommandDotNet;

namespace Automatron.Tasks.Commands;

public record TaskRunOptions : IArgumentModel
{
    [Option('p', Description = "Parameters to use", Split = ',')]
    public IEnumerable<string>? Parameters { get; set; }

    [Option('s', Description = "Dependencies to skip", Split = ',')]
    public IEnumerable<string>? Skip { get; set; }
}
#endif