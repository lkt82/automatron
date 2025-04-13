#if NETSTANDARD2_0
using static Automatron.AzureDevOps.Generators.Models.NuGetToolInstallerTask;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class NuGetToolInstallerTask : Task<NuGetToolInstallerInputs>
{
    public class NuGetToolInstallerInputs
    {
        public string? VersionSpec { get; set; }

        public bool? CheckLatest { get; set; }
    }

    public NuGetToolInstallerTask(IJob job, NuGetToolInstallerInputs? inputs=null) : base(job, "NuGetToolInstaller@1", inputs)
    {
    }
}
#endif