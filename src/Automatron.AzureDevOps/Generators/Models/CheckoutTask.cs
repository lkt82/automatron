using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class CheckoutTask: Step
    {
        public CheckoutTask(IJob job,string source) : base(job)
        {
            Source = source;
        }

        [YamlMember(Alias = "checkout")]
        public string Source { get; set; }
    }
}
