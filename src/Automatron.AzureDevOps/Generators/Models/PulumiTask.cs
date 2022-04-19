namespace Automatron.AzureDevOps.Generators.Models
{
    public class PulumiTask : Task<PulumiTask.PulumiTaskInputs>
    {
        public class PulumiTaskInputs
        {
            public string? Command { get; set; }

            public string? Stack { get; set; }

            public string? Cwd { get; set; }

            public string? Args { get; set; }
        }

        public PulumiTask(PulumiTaskInputs? input=null) : base("Pulumi@1", input)
        {

        }
    }
}
