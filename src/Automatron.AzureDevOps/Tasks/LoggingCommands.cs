#if NET8_0
using System.Collections.Generic;
using CommandDotNet;

namespace Automatron.AzureDevOps.Tasks;

public class LoggingCommands(IConsole console)
{
    public async System.Threading.Tasks.Task UpdateBuildNumberAsync(string buildNumber)
    {
        await console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
    }

    public async System.Threading.Tasks.Task UploadArtifactAsync(string folder,string name,string path)
    {
        await console.Out.WriteLineAsync($"##vso[artifact.upload containerfolder={folder};artifactname={name}]{path}");
    }

    public async System.Threading.Tasks.Task PublishTestResultsAsync(string type, IEnumerable<string> resultFiles, string title,bool mergeResults=false)
    {
        await console.Out.WriteLineAsync($"##vso[results.publish type={type};resultFiles={string.Join(",", resultFiles)};mergeResults={mergeResults};runTitle='{title}']");
    }
}
#endif