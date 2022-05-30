using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public abstract class Task<T> : Step
    {
        protected Task(IJob job,string type,T? inputs) : base(job)
        {
            Type = type;
            Inputs = inputs;
        }

        [YamlMember(Alias = "task")]
        public string Type { get; set; }

        [YamlMember]
        public T? Inputs { get; set; }
    }
}