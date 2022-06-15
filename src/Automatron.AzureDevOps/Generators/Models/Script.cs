using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                return Secrets?.ToDictionary(GetEnvVarName, c=> (object)$"$({c})") ?? _env;
            }
            set => _env = value;
        }

        private static string GetEnvVarName(string name)
        {
            var envVarName = new StringBuilder();

            for (var index = 0; index < name.Length; index++)
            {
                var n = name[index];
                if (index > 0 && char.IsLower(name[index - 1]) && char.IsUpper(n))
                {
                    envVarName.Append('_');
                    envVarName.Append(n);
                }
                else if (char.IsLower(n))
                {
                    envVarName.Append(char.ToUpper(n));
                }
                else
                {
                    envVarName.Append(n);
                }
            }

            return envVarName.ToString();
        }
    }
}
