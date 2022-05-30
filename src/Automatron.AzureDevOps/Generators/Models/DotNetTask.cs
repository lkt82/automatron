namespace Automatron.AzureDevOps.Generators.Models
{
    public class DotNetTask : Task<DotNetTask.DotNetTaskInputs>
    {
        public class DotNetTaskInputs
        {
            public DotNetTaskInputs(string command)
            {
                Command = command;
            }

            public string Command { get; set; }

            public string? WorkingDirectory { get; set; }

            public string? Arguments { get; set; }
        }

        public DotNetTask(IJob job,DotNetTaskInputs input) :base(job,"DotNetCoreCLI@2", input)
        {
     
        }
    }
}