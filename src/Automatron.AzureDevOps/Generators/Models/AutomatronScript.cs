namespace Automatron.AzureDevOps.Generators.Models;

public sealed class AutomatronScript : Script
{
    public AutomatronScript(IJob job, string stepName) : base(job, BuildCommand(job.Stage.Pipeline.Name, job.Stage.Name, job.Name, stepName))
    {
    }

    private static string BuildCommand(string pipeline, string stage, string job, string step)
    {
        return $"dotnet build /nodeReuse:false /p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet && dotnet run --no-build -- run --stage {stage} --job {job} --step {step} {pipeline}";
    }
}