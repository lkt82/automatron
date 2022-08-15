#if NET6_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Spectre.Console;
using TopologicalSorting;

namespace Automatron;

internal class TaskEngine : ITaskEngine
{
    private readonly IAnsiConsole _console;
    private readonly IActionRunner _actionRunner;

    private readonly Dictionary<string, Task> _tasks;

    public TaskEngine(IAnsiConsole console, TaskModel taskModel, IActionRunner actionRunner)
    {
        _console = console;
        _actionRunner = actionRunner;
        _tasks = taskModel.Tasks.ToDictionary(c => c.Name.ToLowerInvariant(), c => c);
    }

    public async Task<int> RunAsync(IEnumerable<string> tasks, IEnumerable<string>? skip = null, bool skipAll = false)
    {
        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;

        var tasksArray = tasks as string[] ?? tasks.ToArray();

        if (!tasksArray.Any())
        {
            tasksArray = _tasks.Where(c => c.Value.Default).Select(c => c.Key).ToArray();

            if (tasksArray.Length > 1)
            {
                _console.MarkupLine($"[grey53]{assemblyName}:[/] [red]More then one default task defined[/]");

                return 1;
            }
        }


        var resolvedTasks = new Dictionary<string, Task>();

        foreach (var task in tasksArray)
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

        if (tasksArray.Length == 0)
        {
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [red]No default task defined[/]");

            return 1;
        }

        var tasksToSkip = BuildSkippedTasks(tasksArray, skip, skipAll);

        var taskGraph = BuildTaskGraph(tasksArray, tasksToSkip);

        var resolvedTasksString = string.Join(" ", resolvedTasks.Values.Select(c => c.Name));

        var runStopWatch = new Stopwatch();
        _console.MarkupLine($"[grey53]{assemblyName}:[/] Starting... [deepskyblue3_1]({resolvedTasksString})[/]");

        runStopWatch.Start();

        var table = new Table();

        table.AddColumn("Task");
        table.AddColumn("Outcome");
        table.AddColumn("Duration");

        var failed = false;

        foreach (OrderedProcess graphItem in taskGraph.CalculateSort())
        {
            var task = _tasks[graphItem.Name];
            var taskStopWatch = new Stopwatch();
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task.Name}[/]: Starting...");
            taskStopWatch.Start();
            try
            {
                await _actionRunner.Run(new TaskContext(task.ActionDescriptor, task.ParameterDescriptors));

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

    private string[] BuildSkippedTasks(IEnumerable<string> tasks, IEnumerable<string>? skip, bool skipAll)
    {
        var tasksToSkip = Array.Empty<string>();

        if (skip != null)
        {
            tasksToSkip = skip.Select(c => c.ToLowerInvariant()).ToArray();
        }

        if (!skipAll) return tasksToSkip;
        {
            var dependencies = new HashSet<string>();
            foreach (var task in tasks.Select(c => c.ToLowerInvariant()))
            {
                dependencies.UnionWith(_tasks[task].Dependencies.Select(c=> c.Name));
            }

            tasksToSkip = dependencies.Select(c => c.ToLowerInvariant()).ToArray();
        }

        return tasksToSkip;
    }

    private void BuildTaskGraph(IEnumerable<string> tasks, DependencyGraph graph, IDictionary<string, OrderedProcess> lookup, string[] skip)
    {
        foreach (var task in tasks.Select(c => c.ToLowerInvariant()))
        {
            var taskCommand = _tasks[task];

            if (lookup.ContainsKey(task))
            {
                continue;
            }

            if (skip.Contains(task))
            {
                continue;
            }

            var graphItem = new OrderedProcess(graph, task);
            lookup.Add(task, graphItem);

            BuildTaskGraph(taskCommand.Dependencies.Select(c => c.Name), graph, lookup, skip);
        }
    }

    private DependencyGraph BuildTaskGraph(IEnumerable<string> tasks, string[] skip)
    {
        var dependencyGraph = new DependencyGraph();
        var nameLookup = new Dictionary<string, OrderedProcess>();


        BuildTaskGraph(tasks, dependencyGraph, nameLookup, skip);

        foreach (var task in nameLookup)
        {
            var graphItem = task.Value;

            var controllerTask = _tasks[task.Key];

            foreach (var dependency in controllerTask.Dependencies.Select(c => c.Name.ToLowerInvariant()))
            {
                if (skip.Contains(dependency))
                {
                    continue;
                }

                nameLookup[dependency].Before(graphItem);
            }
        }

        return dependencyGraph;
    }
}

#endif