using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Script : Step
    {
        public Script(string content)
        {
            Content = content;
        }

        [YamlMember(Alias = "script")]
        public string Content { get; set; }
    }
}
