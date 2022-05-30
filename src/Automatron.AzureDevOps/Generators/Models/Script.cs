using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Script : Step
    {
        private IDictionary<string, object>? _env;

        public Script(IJob job,string content) : base(job)
        {
            Content = content;
        }

        [YamlMember(Alias = "script")]
        public string Content { get; set; }

        [YamlMember]
        public string? WorkingDirectory { get; set; }

        [YamlIgnore]
        public string[]? Secrets { get; set; }

        public IDictionary<string, object>? Env
        {
            get
            {
                return Secrets?.ToDictionary(c=> c,c=> (object)$"$({c})") ?? _env;
            }
            set => _env = value;
        }
    }
}
