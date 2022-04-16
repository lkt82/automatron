namespace Automatron.AzureDevOps.Generators.Models
{
    public class NuGetAuthenticateTask : Task<NuGetAuthenticateTask.NuGetAuthenticateInputs>
    {
        public class NuGetAuthenticateInputs
        {
            public string? NuGetServiceConnections { get; set; }

            public bool? ForceReinstallCredentialProvider { get; set; }
        }

        public NuGetAuthenticateTask() : base("NuGetAuthenticate@0",null)
        {
        }

        public NuGetAuthenticateTask(NuGetAuthenticateInputs inputs) : base("NuGetAuthenticate@0", inputs)
        {
        }
    }
}
