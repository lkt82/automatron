using System;
using System.IO;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutomatronTaskAttribute : StepAttribute
    {
        private const string _displayName = "Run Automatron";

        public AutomatronTaskAttribute()
        {
            DisplayName = _displayName;
        }

        public AutomatronTaskAttribute(string job)
        {
            Job = job;
            DisplayName = _displayName;
        }

        public bool SkipDependencies { get; set; }

        public bool Parallel { get; set; }

        public string? WorkingDirectory { get; set; }

        public string[]? Secrets { get; set; }

        public override Step Create(ISymbol symbol, IJob job)
        {
            return new AutomatronTask(job,new[]{symbol.Name}, SkipDependencies, Parallel) { 
                Name = Name, 
                DisplayName = DisplayName,
                Condition = Condition,
                WorkingDirectory = WorkingDirectory?? GetWorkingDirectory(job),
                Secrets = Secrets
            };
        }

        private static string GetWorkingDirectory(IJob job)
        {
            const string start = "./";
            var path = PathExtensions.GetUnixPath(PathExtensions.GetRelativePath(Path.GetFullPath(Path.Combine(job.Stage.Pipeline.ProjectDirectory, job.Stage.Pipeline.RootPath)), job.Stage.Pipeline.ProjectDirectory));

            return path.StartsWith(start) ? path : $"{start}{path}";
        }
    }
}
