#if NET6_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandDotNet;
using JetBrains.Annotations;
using Spectre.Console;
using TopologicalSorting;

namespace Automatron;

internal sealed class TaskCommand
{
    private readonly Dictionary<string, ControllerTask> _tasks;
    private readonly IAnsiConsole _console;

    public TaskCommand(Dictionary<string, ControllerTask> tasks, IAnsiConsole console)
    {
        _tasks = tasks;
        _console = console;
    }

    private void BuildTaskGraph(IEnumerable<string> tasks,DependencyGraph graph, IDictionary<string, OrderedProcess> lookup, string[] skip)
    {
        foreach (var task in tasks.Select(c=> c.ToLowerInvariant()))
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

            BuildTaskGraph(taskCommand.Dependencies, graph, lookup, skip);
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

            foreach (var dependency in controllerTask.Dependencies.Select(c => c.ToLowerInvariant()))
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

    [DefaultCommand]
    [UsedImplicitly]
    public async Task<int> Execute(
        [Operand(Description = "A list of tasks to run. If not specified, the \"default\" task will be invoked")]
        IEnumerable<string>? tasks,
        [Option('n',Description = "Do a dry run without executing actions")]
        bool? dryRun,
        [Option('p',Description = "Run tasks in parallel")]
        bool? parallel,
        [Option('s',Description = "List of dependencies to be skipped",Split = ',')]
        IEnumerable<string>? skip,
        [Option('a',Description = "Skips all dependencies")]
        bool skipAll,
        [Option('t',Description = "List of tasks to be invoked",Split = ',')]
        IEnumerable<string>? taskList,
        CommandContext ctx
    )
    {
        string[] taskArray = { "default" };

        if (taskList != null)
        {
            taskArray = taskList.ToArray();
        }
        else if(tasks != null)
        {
            taskArray = tasks.ToArray();
        }

        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name;

        var resolvedTasks = new Dictionary<string,ControllerTask>();


        foreach (var task in taskArray)
        {
            var key = task.ToLowerInvariant();

            if (_tasks.TryGetValue(key, out var controllerTask))
            {
                resolvedTasks.Add(key,controllerTask);
                continue;
            }
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task}[/]: [red]Not found[/]");

            return 1;
        }

        var tasksToSkip = BuildSkippedTasks(taskArray, skip, skipAll);

        var dependencyGraph = BuildTaskGraph(taskArray, tasksToSkip);

        var resolvedTasksString = string.Join(" ", resolvedTasks.Values.Select(c => c.Name));

        var runStopWatch = new Stopwatch();
         _console.MarkupLine($"[grey53]{assemblyName}:[/] Starting... [deepskyblue3_1]({resolvedTasksString})[/]");

        runStopWatch.Start();

        var table = new Table();

        table.AddColumn("Task");
        table.AddColumn("Outcome");
        table.AddColumn("Duration");

        var failed = false;


        foreach (OrderedProcess graphItem in dependencyGraph.CalculateSort())
        {
            var task = _tasks[graphItem.Name];
            var taskStopWatch = new Stopwatch();
            _console.MarkupLine($"[grey53]{assemblyName}:[/] [deepskyblue3_1]{task.Name}[/]: Starting...");
            taskStopWatch.Start();
            try
            {
                if (typeof(Task).IsAssignableFrom(task.Action.ReturnType))
                {
                    await (Task)task.Action.Invoke(ctx.DependencyResolver!.Resolve(task.ControllerType), null)!;
                }
                else
                {
                    task.Action.Invoke(ctx.DependencyResolver!.Resolve(task.ControllerType), null);
                }

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

    private string[] BuildSkippedTasks(string[] tasks, IEnumerable<string>? skip, bool skipAll)
    {
        var tasksToSkip = Array.Empty<string>();

        if (skip != null)
        {
            tasksToSkip = skip.Select(c => c.ToLowerInvariant()).ToArray();
        }

        if (skipAll)
        {
            var dependencies = new HashSet<string>();
            foreach (var task in tasks.Select(c => c.ToLowerInvariant()))
            {
                dependencies.UnionWith(_tasks[task].Dependencies);
            }

            tasksToSkip = dependencies.Select(c => c.ToLowerInvariant()).ToArray();
        }

        return tasksToSkip;
    }
}
#endif