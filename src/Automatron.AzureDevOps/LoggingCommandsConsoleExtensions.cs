#if NET6_0
using System.Threading.Tasks;
using CommandDotNet;

namespace Automatron.AzureDevOps
{
    public static class LoggingCommandsConsoleExtensions
    {
        public static async Task UpdateBuildNumber(this IConsole console,string buildNumber)
        {
            await console.Out.WriteLineAsync($"##vso[build.updatebuildnumber]{buildNumber}");
        }
    }
}
#endif
