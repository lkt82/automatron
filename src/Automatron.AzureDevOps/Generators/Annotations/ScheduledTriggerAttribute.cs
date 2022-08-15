using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class ScheduledTriggerAttribute : Attribute
    {
        public ScheduledTriggerAttribute(string cron)
        {
            Cron = cron;
        }

        public string Cron { get; }
            
        public string? DisplayName { get; set; }

        public bool Always { get; set; }

        public string[]? IncludeBranches { get; set; }

        public string[]? ExcludeBranches { get; set; }
    }
}