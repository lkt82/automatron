#if NETSTANDARD2_0
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Pool
{
    public Pool(string? name, string? vmImage)
    {
        Name = name;
        VmImage = vmImage;
    }

    public string? Name { get; set; }

    public string? VmImage { get; set; }
}
#endif