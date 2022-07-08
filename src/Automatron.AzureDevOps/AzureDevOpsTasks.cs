#if NET6_0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandDotNet;

namespace Automatron.AzureDevOps
{
    public class AzureDevOpsTasks
    {
        private readonly IConsole _console;

        public AzureDevOpsTasks(IConsole console)
        {
            _console = console;
        }

        public async Task UpdateBuildNumberAsync(string buildNumber)
        {
            await _console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
        }

        public async Task UploadArtifactAsync(string folder,string name,string path)
        {
            await _console.Out.WriteLineAsync($"##vso[artifact.upload containerfolder={folder};artifactname={name}]{path}");
        }

        public async Task PublishTestResultsAsync(string type, IEnumerable<string> resultFiles, string title,bool mergeResults=false)
        {
            await Console.Out.WriteLineAsync($"##vso[results.publish type={type};resultFiles={string.Join(",", resultFiles)};mergeResults={mergeResults};runTitle='{title}']");
            //await _console.Out.WriteLineAsync($"##vso[results.publish type={type};resultFiles={string.Join(",", resultFiles)};mergeResults={mergeResults};runTitle='{title}']");
        }
    }
}
#endif