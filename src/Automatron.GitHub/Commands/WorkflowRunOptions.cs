#if NET8_0
namespace Automatron.GitHubActions.Commands;

public class WorkflowRunOptions
{
    public bool DryRun { get; set; }
    public string? WorkflowName { get; set; }
    public string? JobName { get; set; }
    public string? StepName { get; set; }
}
#endif