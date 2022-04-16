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

        public ScheduledTriggerAttribute(string pipeline, string cron) :this(cron)
        {
            Pipeline = pipeline;
        }

        public string? Pipeline { get; }

        public string Cron { get; }
            
        public string? DisplayName { get; set; }

        public bool Always { get; set; }

        public string[]? IncludeBranches { get; set; }

        public string[]? ExcludeBranches { get; set; }
    }
}