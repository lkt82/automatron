#if NETSTANDARD2_0
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class CheckoutTask: Step
{
    public CheckoutTask(IJob job,string source) : base(job)
    {
        Source = source;
    }

    [YamlMember(Alias = "checkout")]
    public string Source { get; set; }

    public int? FetchDepth { get; set; }

    public bool? Clean { get; set; }

    public bool? Lfs { get; set; }

    public bool? Submodules { get; set; }

    public string? Path { get; set; }

    public bool? PersistCredentials { get; set; }
}
#endif