using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Models;

public class Script : Step
{
    public Script(IJob job,string content) : base(job)
    {
        Content = content;
    }

    [YamlMember(Alias = "script")]
    public string Content { get; set; }

    [YamlMember]
    public string? WorkingDirectory { get; set; }

    public IDictionary<string, object>? Env { get; set; }
}