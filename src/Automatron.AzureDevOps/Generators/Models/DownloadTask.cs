using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class DownloadTask : Step
{
    public DownloadTask(IJob job,string source) : base(job)
    {
        Source = source;
    }

    [YamlMember(Alias = "download")]
    public string Source { get; set; }

    public string? Artifact { get; set; }

    public string? Patterns { get; set; }
}