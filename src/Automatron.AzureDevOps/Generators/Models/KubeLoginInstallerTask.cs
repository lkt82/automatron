#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class KubeLoginInstallerTask : Task<KubeLoginInstallerTask.KubeLoginInstallerInputs>
{
    public class KubeLoginInstallerInputs
    {
        public string? KubeLoginVersion { get; set; }
    }

    public KubeLoginInstallerTask(IJob job, KubeLoginInstallerInputs? inputs=null) : base(job, "KubeloginInstaller@0", inputs)
    {
    }
}
#endif