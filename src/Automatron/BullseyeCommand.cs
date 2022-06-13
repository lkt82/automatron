using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using CommandDotNet;
using JetBrains.Annotations;

namespace Automatron
{
    internal sealed class BullseyeCommand
    {
        private readonly Targets _bullseyeService;
        private readonly IConsole _console;

        public BullseyeCommand(Targets bullseyeService, IConsole console)
        {
            _bullseyeService = bullseyeService;
            _console = console;
        }

        [DefaultCommand]
        [UsedImplicitly]
        public async Task Execute(
            [Operand(Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.")]
            IEnumerable<string>? targets,
            [Option('n',Description = "Do a dry run without executing actions")]
            bool? dryRun,
            [Option('d',Description = "List all (or specified) targets and dependencies, then exit")]
            bool? listDependencies,
            [Option('l',Description = "List all (or specified) targets, then exit")]
            bool? listTargets,
            [Option('t',Description = "List all (or specified) targets and dependency trees, then exit")]
            bool? listTree,
            [Option('p',Description = "Run targets in parallel")]
            bool? parallel,
            [Option('s',Description = "Do not run targets' dependencies")]
            bool? skipDependencies,
            [Option('r',Description = "Run a list of targets")]
            IEnumerable<string>? run
        )
        {
            var options = new Options
            {
                Clear = false,//clear ?? false,
                DryRun = dryRun ?? false,
                Host = Host.Automatic,
                ListDependencies = listDependencies ?? false,
                ListInputs = false,//listInputs ?? false,
                ListTargets = listTargets ?? false,
                ListTree = listTree ?? false,
                NoColor = false,
                Parallel = parallel ?? false,
                SkipDependencies = skipDependencies ?? false,
                Verbose = false
            };

            try
            {
                await _bullseyeService.RunAndExitAsync(run ?? targets?? Enumerable.Empty<string>(), options, outputWriter: _console.Out, diagnosticsWriter: _console.Error);
            }
            catch (InvalidUsageException exception)
            {
                await _console.Error.WriteLineAsync(exception.Message);
            }
        }
    }
}