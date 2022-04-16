namespace Automatron.AzureDevOps.Generators.Models
{
    public class ScheduledTrigger
    {
        public ScheduledTrigger(string cron)
        {
            Cron = cron;
        }

        public string Cron { get; set; }

        public string? DisplayName { get; set; }

        public bool? Always { get; set; }

        public TriggerBranches? Branches { get; set; }
    }
}
