namespace Automatron.AzureDevOps.Models;

public sealed class NuGetAuthenticateTask : Task<NuGetAuthenticateTask.NuGetAuthenticateInputs>
{
    public class NuGetAuthenticateInputs
    {
        public string? NuGetServiceConnections { get; set; }

        public bool? ForceReinstallCredentialProvider { get; set; }
    }

    public NuGetAuthenticateTask(IJob job,NuGetAuthenticateInputs? inputs=null) : base(job,"NuGetAuthenticate@0", inputs)
    {
    }
}