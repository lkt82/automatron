namespace Automatron.AzureDevOps.Generators.Models
{
    public class AutomatronTask : Script
    {
        public AutomatronTask(IJob job, string[] targets, bool skipDependencies = false, bool parallel = false) : base(job, $"dotnet run -- {Arguments(targets, skipDependencies, parallel)}")
        {
        }

        private static string Arguments(string[] targets, bool skipDependencies, bool parallel)
        {
            var arguments = string.Join(" ", targets);

            if (skipDependencies)
            {
                arguments += " -s";
            }
            if (parallel)
            {
                arguments += " -p";
            }

            return arguments;
        }
    }
}
