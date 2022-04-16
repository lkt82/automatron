using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class VariableGroup : IVariable
    {
        public VariableGroup(string name)
        {
            Name = name;
        }

        [YamlMember(Alias = "group")]
        public string Name { get; set; }
    }
}
