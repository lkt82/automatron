#if NET8_0
using System.Collections.Generic;
using CommandDotNet;

namespace Automatron.Tasks.Commands;

public record TaskRunArgs : IArgumentModel
{
    [Operand(Description = "tasks to run")]
    public IEnumerable<string>? Tasks { get; set; }
}
#endif