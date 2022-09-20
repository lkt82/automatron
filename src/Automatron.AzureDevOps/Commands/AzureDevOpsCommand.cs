#if NET6_0
using System;
using CommandDotNet;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using Automatron.AzureDevOps.Models;
using System.Threading.Tasks;

namespace Automatron.AzureDevOps.Commands
{
    [Command("AzureDevOps", Description = "AzureDevOps commands")]
    public class AzureDevOpsCommand
    {
        private readonly IConsole _console3;
        private readonly IAnsiConsole _console;
        private readonly IEnumerable<Pipeline> _pipelines;
        private readonly IPipelineEngine _pipelineEngine;
        private readonly Table _summeryTable;

        public AzureDevOpsCommand(IConsole console3,IAnsiConsole console, IEnumerable<Pipeline> pipelines, IPipelineEngine pipelineEngine)
        {
            _console3 = console3;
            _console = console;
            _pipelines = pipelines;
            _pipelineEngine = pipelineEngine;
            _pipelineEngine.OnPipelineCompleted += PipelineEngineOnPipelineCompleted;
            _pipelineEngine.OnStageCompleted += PipelineEngineOnStageCompleted;
            _pipelineEngine.OnJobCompleted += PipelineEngineOnJobCompleted;
            _pipelineEngine.OnStepCompleted += PipelineEngineOnStepCompleted;
            _pipelineEngine.OnStepFailed += PipelineEngineOnStepFailed;
            _pipelineEngine.OnJobFailed += PipelineEngineOnJobFailed;
            _pipelineEngine.OnStageFailed += PipelineEngineOnStageFailed;

            _summeryTable = new Table();
            _summeryTable.Border(TableBorder.Ascii2);
            _summeryTable.AddColumn("Pipeline");
            _summeryTable.AddColumn("Stage");
            _summeryTable.AddColumn("Job");
            _summeryTable.AddColumn("Step");
            _summeryTable.AddColumn("Outcome");
            _summeryTable.AddColumn("Duration");
        }

        private void PipelineEngineOnStageFailed(object? sender, PipelineModelFailedArgs<Stage> args)
        {
            _summeryTable.AddRow(args.Model.Pipeline.Name, $"[deepskyblue3_1]{args.Model.Name}[/]", string.Empty, string.Empty, "[red]FAILED[/]", $"[purple]{args.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnJobFailed(object? sender, PipelineModelFailedArgs<Job> args)
        {
            _summeryTable.AddRow(args.Model.Stage.Pipeline.Name, args.Model.Stage.Name, $"[deepskyblue3_1]{args.Model.Name}[/]", string.Empty, "[red]FAILED[/]", $"[purple]{args.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnStepFailed(object? sender, PipelineModelFailedArgs<Step> args)
        {
            var executionName = $"{args.Model.Job.Stage.Pipeline.Name}.{args.Model.Job.Stage.Name}.{args.Model.Job.Name}";

            var error = Markup.Escape(args.Exception.GetBaseException().ToString());

            _console.MarkupLine($"[grey53]{executionName}:[/] [deepskyblue3_1]{args.Model.Name}[/]: [red]{error}[/]");

            _summeryTable.AddRow(args.Model.Job.Stage.Pipeline.Name, args.Model.Job.Stage.Name, args.Model.Job.Name, $"[deepskyblue3_1]{args.Model.Name}[/]", "[red]FAILED[/]", $"[purple]{args.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnStepCompleted(object? sender, PipelineModelCompletedArgs<Step> args)
        {
            var executionName = $"{args.Model.Job.Stage.Pipeline.Name}.{args.Model.Job.Stage.Name}.{args.Model.Job.Name}";

            _console.MarkupLine($"[grey53]{executionName}.[/][deepskyblue3_1]{args.Model.Name}[/]: [green]Succeeded[/]: [purple]({args.Elapsed.Milliseconds} ms)[/]");
            _summeryTable.AddRow(args.Model.Job.Stage.Pipeline.Name, args.Model.Job.Stage.Name, args.Model.Job.Name, $"[deepskyblue3_1]{args.Model.Name}[/]", "[green]Succeeded[/]", $"[purple]{args.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnJobCompleted(object? sender, PipelineModelCompletedArgs<Job> e)
        {
            _summeryTable.AddRow(e.Model.Stage.Pipeline.Name, e.Model.Stage.Name, $"[deepskyblue3_1]{e.Model.Name}[/]", string.Empty, "[green]Succeeded[/]", $"[purple]{e.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnStageCompleted(object? sender, PipelineModelCompletedArgs<Stage> e)
        {
            _summeryTable.AddRow(e.Model.Pipeline.Name, $"[deepskyblue3_1]{e.Model.Name}[/]", string.Empty, string.Empty, "[green]Succeeded[/]", $"[purple]{e.Elapsed.Milliseconds} ms[/]");
        }

        private void PipelineEngineOnPipelineCompleted(object? sender, PipelineModelCompletedArgs<Pipeline> e)
        {
            _summeryTable.AddRow($"[deepskyblue3_1]{e.Model.Name}[/]", string.Empty, string.Empty, string.Empty, "[green]Succeeded[/]", $"[purple]{e.Elapsed.Milliseconds} ms[/]");
        }


        [Command( Description = "Run Azure DevOps pipeline")]
        public async Task<int> Run(PipelineRunArgs args, PipelineRunOptions options)
        {
            var foundPipeline =_pipelines.FirstOrDefault(c => string.Equals(c.Name, args.Pipeline, StringComparison.InvariantCultureIgnoreCase));

            if (foundPipeline == null)
            {
                _console.MarkupLine($"Pipeline [deepskyblue3_1]{args.Pipeline}[/]: [red]Not found[/]");

                return 1;
            }

            var foundStage = !string.IsNullOrEmpty(options.Stage) ? foundPipeline.Stages.FirstOrDefault(c => string.Equals(c.Name, options.Stage, StringComparison.InvariantCultureIgnoreCase)) : foundPipeline.Stages.FirstOrDefault();

            if (foundStage == null)
            {
                _console.MarkupLine($"Stage [deepskyblue3_1]{options.Stage}[/]: [red]Not found[/]");

                return 1;
            }

            var foundJob = !string.IsNullOrEmpty(options.Job) ? foundStage.Jobs.FirstOrDefault(c => string.Equals(c.Name, options.Job, StringComparison.InvariantCultureIgnoreCase)) : foundStage.Jobs.FirstOrDefault();

            if (foundJob == null)
            {
                _console.MarkupLine($"Job [deepskyblue3_1]{options.Job}[/]: [red]Not found[/]");

                return 1;
            }

            Step? foundStep = null;

            if (!string.IsNullOrEmpty(options.Step))
            {
                foundStep = foundJob.Steps.FirstOrDefault(c =>
                    string.Equals(c.Name, options.Step, StringComparison.InvariantCultureIgnoreCase));

                if (foundStep == null)
                {
                    _console.MarkupLine($"Step [deepskyblue3_1]{options.Step}[/]: [red]Not found[/]");

                    return 1;
                }
            }


            var startingTitle = foundPipeline.Name;
            if (!string.IsNullOrEmpty(options.Stage))
            {
                startingTitle += " " + foundStage.Name;
            }
            if (!string.IsNullOrEmpty(options.Job))
            {
                startingTitle += " " + foundStage.Name;
                startingTitle += " " + foundJob.Name;
            }
            if (foundStep != null)
            {
                startingTitle += " " + foundStage.Name;
                startingTitle += " " + foundJob.Name;
                startingTitle += " " + foundStep.Name;
            }
            _console.MarkupLine($"Starting... [deepskyblue3_1]({startingTitle})[/]");

            try
            {
                PipelineResult result;

                if (!string.IsNullOrEmpty(options.Step))
                {
                   result = await _pipelineEngine.Run(foundStep!, options.Variables,args.Parameters);
                }
                else if (!string.IsNullOrEmpty(options.Job))
                {
                   result = await _pipelineEngine.Run(foundJob, options.Variables, args.Parameters);
                }
                else if (!string.IsNullOrEmpty(options.Stage))
                {
                   result = await _pipelineEngine.Run(foundStage, options.Variables, args.Parameters);
                }
                else
                {
                   result = await _pipelineEngine.Run(foundPipeline, options.Variables, args.Parameters);
                }
                _console.Write(_summeryTable);
                _console.MarkupLine($"[green]Succeeded[/] [deepskyblue3_1]({startingTitle}[/] [purple]({result!.Elapsed.Milliseconds} ms))[/]");
                await _console3.Out.FlushAsync();

                return 0;
            }
            catch (PipelineException exception)
            {
                _console.Write(_summeryTable);
                _console.MarkupLine($"[red]FAILED![/] [deepskyblue3_1]({startingTitle}[/] [purple]({exception.Elapsed.Milliseconds} ms))[/]");
                return 1;
            }

        }
    }
}
#endif