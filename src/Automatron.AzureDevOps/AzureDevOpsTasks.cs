#if NET6_0
using System.Reflection;
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

        public async Task UpdateBuildNumber(string buildNumber)
        {
            await _console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
        }

        public async Task UpdateBuildNumberWithAssemblyInformationalVersion()
        {
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (version == null)
            {
                return;
            }

            await UpdateBuildNumber(version);
        }

        public async Task UploadArtifact(string folder,string name,string path)
        {
            await _console.Out.WriteLineAsync($"##vso[artifact.upload containerfolder={folder};artifactname={name}]{path}");
        }
    }
}
#endif