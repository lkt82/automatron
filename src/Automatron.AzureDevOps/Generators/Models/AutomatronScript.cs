namespace Automatron.AzureDevOps.Generators.Models;

public sealed class AutomatronScript : Script
{
    public AutomatronScript(IJob job, string stepName) : base(job, BuildCommand(job.Stage.Pipeline.Name, job.Stage.Name, job.Name, stepName))
    {
    }

    private static string BuildCommand(string pipeline, string stage, string job, string step)
    {
        return $"dotnet run -- run {pipeline} -s {stage} -j {job} -t {step}";
    }
}