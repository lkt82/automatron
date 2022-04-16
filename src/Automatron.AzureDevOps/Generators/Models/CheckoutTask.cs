using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class CheckoutTask: Step
    {
        public CheckoutTask(string source)
        {
            Source = source;
        }

        [YamlMember(Alias = "checkout")]
        public string Source { get; set; }
    }
}
