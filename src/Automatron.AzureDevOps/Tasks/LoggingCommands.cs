#if NET6_0
using System.Collections.Generic;
using CommandDotNet;

namespace Automatron.AzureDevOps.Tasks
{
    public class LoggingCommands
    {
        private readonly IConsole _console;

        public LoggingCommands(IConsole console)
        {
            _console = console;
        }

        public async System.Threading.Tasks.Task UpdateBuildNumberAsync(string buildNumber)
        {
            await _console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
        }

        public async System.Threading.Tasks.Task UploadArtifactAsync(string folder,string name,string path)
        {
            await _console.Out.WriteLineAsync($"##vso[artifact.upload containerfolder={folder};artifactname={name}]{path}");
        }

        public async System.Threading.Tasks.Task PublishTestResultsAsync(string type, IEnumerable<string> resultFiles, string title,bool mergeResults=false)
        {
            await _console.Out.WriteLineAsync($"##vso[results.publish type={type};resultFiles={string.Join(",", resultFiles)};mergeResults={mergeResults};runTitle='{title}']");
        }
    }
}
#endif