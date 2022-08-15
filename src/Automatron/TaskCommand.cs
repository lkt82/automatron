#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet;
using JetBrains.Annotations;

namespace Automatron;

internal sealed class TaskCommand
{
    private readonly ITaskEngine _taskEngine;

    public TaskCommand(ITaskEngine taskEngine)
    {
        _taskEngine = taskEngine;
    }

    [DefaultCommand]
    [UsedImplicitly]
    public async Task<int> Execute(
        [Operand(Description = "A list of tasks to run")]
        IEnumerable<string>? tasks,
        [Option('s', Description = "List of dependencies to be skipped", Split = ',')]
        IEnumerable<string>? skip,
        [Option('a', Description = "Skips all dependencies")]
        bool skipAll,
        [Option('t', Description = "List of tasks to be invoked", Split = ',')]
        IEnumerable<string>? taskList
    )
    {
        var taskArray = Array.Empty<string>();

        if (taskList != null)
        {
            taskArray = taskList.ToArray();
        }
        else if (tasks != null)
        {
            taskArray = tasks.ToArray();
        }

        return await _taskEngine.RunAsync(taskArray, skip);
    }
}
#endif