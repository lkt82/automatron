using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StepAttribute : NodeAttribute
    {
        public string? WorkingDirectory { get; set; }

        public string[]? DependsOn { get; set; }
    }
}
