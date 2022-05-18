using System;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class AutomatronTaskEnv : Attribute
    {
        public AutomatronTaskEnv(string key,string value)
        {

        }

    }

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

        public string[]? Secrets { get; set; }

        public override Step Create(ISymbol symbol)
        {
            return new AutomatronTask(new []{symbol.Name}, SkipDependencies, Parallel) { 
                Name = Name, 
                DisplayName = DisplayName,
                Condition = Condition,
                Secrets = Secrets
            };
        }
    }
}
