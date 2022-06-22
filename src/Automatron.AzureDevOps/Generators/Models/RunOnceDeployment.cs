namespace Automatron.AzureDevOps.Generators.Models;

public sealed class RunOnceDeployment
{
    public Deploy Deploy { get; set; } = new();
}