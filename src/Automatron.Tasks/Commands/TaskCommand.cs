#if NET6_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Automatron.Collections;
using Automatron.Tasks.Models;
using CommandDotNet;
using Spectre.Console;
using Task = Automatron.Tasks.Models.Task;

namespace Automatron.Tasks.Commands;

[Command("Task",Description = "Task commands")]
public class TaskCommand
{
    private readonly IAnsiConsole _console;
    private readonly ITaskRunner _taskRunner;

    private readonly Dictionary<string, Task> _tasks;

    public TaskCommand(IAnsiConsole console, IEnumerable<Task> tasks, ITaskRunner taskRunner)
    {
        _console = console;
        _taskRunner = taskRunner;

        _tasks = tasks.ToDictionary(c => c.Name.ToLowerInvariant(), c => c);
    }

    [Command(Description = "Run tasks")]
    public async Task<int> Run(TaskRunArgs args, TaskRunOptions options)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;

        var taskNames = (args.Tasks ?? Array.Empty<string>()).ToArray();

        if (!taskNames.Any())
        {
            taskNames = _tasks.Where(c => c.Value.Default).Select(c => c.Key).ToArray();

            if (taskNames.Length > 1)
            {
                _console.MarkupLine($"[grey53]{assemblyName}:[/] [red]More then one default task defined[/]");

                return 1;
            }
        }


        var resolvedTasks = new Dictionary<string, Task>();

        foreach (var task in taskNames)
        {
            var key = task.ToLowerInvariant();

            if (_tasks.TryGetValue(key, out var controllerTask))
            {
                resolvedTasks.Add(key, controllerTask);
                continue;
            }
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task}[/]: [red]Not found[/]");

            return 1;
        }

        if (taskNames.Length == 0)
        {
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [red]No default task defined[/]");

            return 1;
        }

        var tasksNamesToSkip = BuildSkippedTasks(taskNames, options.Skip, options.Skip != null && !options.Skip.Any());

        var tasksToRun = BuildTasks(taskNames);

        var resolvedTasksString = string.Join(" ", resolvedTasks.Values.Select(c => c.Name));

        var runStopWatch = new Stopwatch();
        _console.MarkupLine($"[grey53]{assemblyName}:[/] Starting... [deepskyblue3_1]({resolvedTasksString})[/]");

        runStopWatch.Start();

        var table = new Table();

        table.AddColumn("Task");
        table.AddColumn("Outcome");
        table.AddColumn("Duration");

        var failed = false;

        var sorted = tasksToRun.TopologicalSort(x => x.Dependencies).Where(c => !tasksNamesToSkip.Contains(c));

        foreach (var task in sorted)
        {
            var taskStopWatch = new Stopwatch();
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task.Name}[/]: Starting...");
            taskStopWatch.Start();
            try
            {
                await _taskRunner.Run(task);

                taskStopWatch.Stop();
                _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task.Name}[/]: [green]Succeeded[/]: [purple]({taskStopWatch.ElapsedMilliseconds} ms)[/]");
                table.AddRow($"[deepskyblue3_1]{task.Name}[/]", "[green]Succeeded[/]", $"[purple]{taskStopWatch.ElapsedMilliseconds} ms[/]");
            }
            catch (Exception e)
            {
                taskStopWatch.Stop();
                var error = Markup.Escape(e.ToString());

                _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task.Name}[/]: [red]{error}[/]");
                table.AddRow($"[deepskyblue3_1]{task.Name}[/]", "[red]FAILED[/]", $"[purple]{taskStopWatch.ElapsedMilliseconds} ms[/]");

                failed = true;
                break;
            }
        }

        runStopWatch.Stop();

        table.Border(TableBorder.Ascii2);

        var outerTable = new Table();
        outerTable.NoBorder();
        outerTable.ShowHeaders = false;
        outerTable.AddColumn("assemblyName");
        outerTable.AddColumn("result");
        outerTable.AddRow(new Markup($"[grey53]{assemblyName}:[/]"), table);
        _console.Write(outerTable);

        _console.MarkupLine(failed
            ? $"[grey53]{assemblyName}:[/] [red]FAILED![/] [deepskyblue3_1]({resolvedTasksString}[/] [purple]({runStopWatch.ElapsedMilliseconds} ms))[/]"
            : $"[grey53]{assemblyName}:[/] [green]Succeeded[/] [deepskyblue3_1]({resolvedTasksString}[/] [purple]({runStopWatch.ElapsedMilliseconds} ms))[/]");

        return failed ? 1 : 0;
    }

    private IEnumerable<Task> BuildSkippedTasks(IEnumerable<string> tasks, IEnumerable<string>? skip, bool skipAll)
    {
        var tasksToSkip = Array.Empty<string>();

        if (skip != null)
        {
            tasksToSkip = skip.Select(c => c.ToLowerInvariant()).ToArray();
        }

        if (!skipAll) return tasksToSkip.Select(c => _tasks[c]);
        {
            var dependencies = new HashSet<string>();
            foreach (var task in tasks.Select(c => c.ToLowerInvariant()))
            {
                dependencies.UnionWith(_tasks[task].Dependencies.Select(c => c.Name));
            }

            tasksToSkip = dependencies.Select(c => c.ToLowerInvariant()).ToArray();
        }

        return tasksToSkip.Select(c => _tasks[c]);
    }

    private void BuildTasks(IEnumerable<string> tasks, IDictionary<string, Task> lookup)
    {
        foreach (var taskName in tasks.Select(c => c.ToLowerInvariant()))
        {
            var task = _tasks[taskName];

            if (lookup.ContainsKey(taskName))
            {
                continue;
            }


            lookup.Add(taskName, task);


            BuildTasks(task.Dependencies.Select(c => c.Name), lookup);
        }
    }

    private IEnumerable<Task> BuildTasks(IEnumerable<string> tasks)
    {
        var nameLookup = new Dictionary<string, Task>();

        BuildTasks(tasks, nameLookup);

        return nameLookup.Values;
    }
}
#endif