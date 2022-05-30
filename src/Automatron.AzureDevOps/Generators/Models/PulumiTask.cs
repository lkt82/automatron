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

        public PulumiTask(IJob job,PulumiTaskInputs? input=null) : base(job,"Pulumi@1", input)
        {

        }
    }
}
