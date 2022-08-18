using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public sealed class VariableGroup : IVariable
{
    public VariableGroup(string name)
    {
        Name = name;
    }

    [YamlMember(Alias = "group")]
    public string Name { get; set; }
}