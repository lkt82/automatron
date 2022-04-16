namespace Automatron.AzureDevOps.Generators.Models
{
    public class CiTrigger: ICiTrigger
    {
        public bool? Batch { get; set; }

        public TriggerBranches? Branches { get; set; }
    }
}