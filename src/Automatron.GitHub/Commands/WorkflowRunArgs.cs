#if NET8_0
namespace Automatron.GitHubActions.Commands;

public class WorkflowRunArgs
{
    public WorkflowRunOptions Options { get; set; } = new();
    public string[] Arguments { get; set; } = [];
}
#endif