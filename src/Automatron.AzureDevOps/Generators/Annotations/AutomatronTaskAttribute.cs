using System;
using System.IO;
using System.Linq;
using System.Text;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutomatronTaskAttribute : StepAttribute
    {
        public AutomatronTaskAttribute()
        {
        }

        public AutomatronTaskAttribute(string job)
        {
            Job = job;
        }

        public bool SkipDependencies { get; set; }

        public bool Parallel { get; set; }

        public string? WorkingDirectory { get; set; }

        public string? Emoji { get; set; }

        public override Step Create(ISymbol symbol, IJob job)
        {
            var target = string.Concat(job.TemplateName,symbol.Name);

            var name = string.IsNullOrEmpty(Name) ? symbol.Name : Name;

            return new AutomatronTask(job,new[]{ target }, SkipDependencies, Parallel, job.Stage.Pipeline.Parameters.Select(c => c.Name).ToArray()) { 
                Name = name, 
                DisplayName = string.IsNullOrEmpty(Emoji) ? DisplayName: $"{Emoji} {name}",
                Condition = Condition,
                WorkingDirectory = WorkingDirectory?? GetWorkingDirectory(job),
                Env = job.Stage.Pipeline.Secrets.ToDictionary(GetEnvVarName, c => (object)$"$({c})")
            };
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

        private static string GetWorkingDirectory(IJob job)
        {
            var fullRoot = PathExtensions.GetUnixPath(Path.GetFullPath(job.Stage.Pipeline.RootPath))+"/";

            var path = PathExtensions.GetUnixPath(PathExtensions.GetRelativePath(fullRoot, job.Stage.Pipeline.ProjectDirectory));

            return path;
        }
    }
}
