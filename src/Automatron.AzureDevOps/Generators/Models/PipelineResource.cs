#if NETSTANDARD2_0
using System.Collections.Generic;
using YamlDotNet.Serialization;
namespace Automatron.AzureDevOps.Generators.Models;

public sealed class PipelineResource
{
    public PipelineResource(string name)
    {
        Name = name;
    }

    [YamlMember(Alias = "pipeline")]
    public string Name { get; }

    public string Source { get; set; }

    public string Project { get; set; }

    public IPipelineResourceTrigger? Trigger { get; set; }
}

public interface IPipelineResourceTrigger
{

}

public sealed class AnyPipelineResourceTrigger : IPipelineResourceTrigger
{
}

public sealed class PipelineResourceTrigger : IPipelineResourceTrigger
{
    public string[]? Stages { get; set; }

    public string[]? Tags { get; set; }

    public TriggerBranches? Branches { get; set; }
}

public sealed class Resources
{
    public IEnumerable<PipelineResource>? Pipelines { get; set; }
}


#endif