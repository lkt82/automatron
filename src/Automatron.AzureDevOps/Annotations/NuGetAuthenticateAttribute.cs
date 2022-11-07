using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NuGetAuthenticateAttribute : NodeAttribute
    {
        public string? NugetServiceConnections { get; set; }

        public bool ReinstallCredentialProvider { get; set; }
    }
}
