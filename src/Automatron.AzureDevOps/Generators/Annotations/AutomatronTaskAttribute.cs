using System;
using System.IO;
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

        public string[]? Secrets { get; set; }

        public override Step Create(ISymbol symbol, IJob job)
        {
            var target = string.Concat(job.Template,symbol.Name);

            var name = string.IsNullOrEmpty(Name) ? symbol.Name : Name;

            return new AutomatronTask(job,new[]{ target }, SkipDependencies, Parallel) { 
                Name = name, 
                DisplayName = DisplayName,
                Condition = Condition,
                WorkingDirectory = WorkingDirectory?? GetWorkingDirectory(job),
                Secrets = Secrets
            };
        }

        private static string GetWorkingDirectory(IJob job)
        {
            var fullRoot = PathExtensions.GetUnixPath(Path.GetFullPath(job.Stage.Pipeline.RootPath))+"/";

            var path = PathExtensions.GetUnixPath(PathExtensions.GetRelativePath(fullRoot, job.Stage.Pipeline.ProjectDirectory));

            return path;
        }
    }
}
