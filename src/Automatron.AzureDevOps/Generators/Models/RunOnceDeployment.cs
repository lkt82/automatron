#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class RunOnceDeployment
{
    public Deploy Deploy { get; set; } = new();
}
#endif