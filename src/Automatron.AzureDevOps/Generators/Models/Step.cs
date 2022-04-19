namespace Automatron.AzureDevOps.Generators.Models
{
    public abstract class Step
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Condition { get; set; }
    }
}
