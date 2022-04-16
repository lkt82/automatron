using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class DownloadTask : Step
    {
        public DownloadTask(string source)
        {
            Source = source;
        }

        [YamlMember(Alias = "download")]
        public string Source { get; set; }

        public string? Artifact { get; set; }

        public string? Patterns { get; set; }
    }
}