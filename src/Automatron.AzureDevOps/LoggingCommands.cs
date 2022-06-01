#if NET6_0
using System.Reflection;
using System.Threading.Tasks;
using CommandDotNet;

namespace Automatron.AzureDevOps
{
    public static class LoggingCommands
    {
        public static async Task UpdateBuildNumber(this IConsole console,string buildNumber)
        {
            await console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
        }

        public static async Task UpdateBuildNumberWithAssemblyInformationalVersion(this IConsole console)
        {
            var version = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (version == null)
            {
                return;
            }

            await console.UpdateBuildNumber(version);
        }
    }
}
#endif
