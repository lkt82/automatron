using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class AutomatronTaskAttribute : StepAttribute
    {
        private const string TaskName = "Run Automatron";

        public AutomatronTaskAttribute()
        {
            DisplayName = TaskName;
        }

        public AutomatronTaskAttribute(string job)
        {
            Job = job;
            DisplayName = TaskName;
        }

        public bool SkipDependencies { get; set; }

        public bool Parallel { get; set; }

        public override Step Create(ISymbol symbol)
        {
            return new AutomatronTask(new []{symbol.Name}, SkipDependencies, Parallel) { 
                Name = Name, 
                DisplayName = DisplayName
            };
        }
    }
}
