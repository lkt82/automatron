namespace Automatron.AzureDevOps.Models;

public sealed class RunOnceDeployment
{
    public Deploy Deploy { get; set; } = new();
}