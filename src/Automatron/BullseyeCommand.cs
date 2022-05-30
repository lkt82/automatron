using System.Linq;
using System.Threading.Tasks;
using Bullseye;
using CommandDotNet;
using JetBrains.Annotations;

namespace Automatron
{
    internal class BullseyeCommand
    {
        [DefaultCommand]
        [UsedImplicitly]
        public async Task Execute(
            [Operand(Description = "A list of targets to run or list. If not specified, the \"default\" target will be run, or all targets will be listed.")]
            string[]? targets,
            [Option('c',Description = "Clear the console before execution")] 
            bool? clear,
            [Option('n',Description = "Do a dry run without executing actions")] 
            bool? dryRun,
            [Option('d',Description = "List all (or specified) targets and dependencies, then exit")]
            bool? listDependencies,
            [Option('i',Description = "List all (or specified) targets and inputs, then exit")]
            bool? listInputs,
            [Option('l',Description = "List all (or specified) targets, then exit")]
            bool? listTargets,
            [Option('t',Description = "List all (or specified) targets and dependency trees, then exit")]
            bool? listTree,
            [Option('p',Description = "Run targets in parallel")] 
            bool? parallel,
            [Option('s',Description = "Do not run targets' dependencies")]
            bool? skipDependencies,
            Targets bullseyeService,
            IConsole console
        )
        {
            var options = new Options
            {
                Clear = clear ?? false,
                DryRun = dryRun ?? false,
                Host = Host.Automatic,
                ListDependencies = listDependencies ?? false,
                ListInputs = listInputs ?? false,
                ListTargets = listTargets ?? false,
                ListTree = listTree ?? false,
                NoColor = false,
                Parallel = parallel ?? false,
                SkipDependencies = skipDependencies ?? false,
                Verbose = false
            };

            try
            {
                await bullseyeService.RunWithoutExitingAsync(targets ?? Enumerable.Empty<string>(), options, outputWriter: console.Out, diagnosticsWriter: console.Error);
            }
            catch (InvalidUsageException exception)
            {
                await console.Error.WriteLineAsync(exception.Message);
                throw;
            }
        }
    }
}