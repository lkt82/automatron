using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public abstract class Task<T> : Step
    {
        protected Task(string type,T? inputs)
        {
            Type = type;
            Inputs = inputs;
        }

        [YamlMember(Alias = "task")]
        public string Type { get; set; }

        public T? Inputs { get; set; }
    }
}