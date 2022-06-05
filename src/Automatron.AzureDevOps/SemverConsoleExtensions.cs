#if NET6_0
using System.Reflection;
using System.Threading.Tasks;
using CommandDotNet;

namespace Automatron.AzureDevOps;

public static class SemverConsoleExtensions
{
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
#endif