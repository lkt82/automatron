#if NET8_0

using Automatron.GitHub.Models;
using CommandDotNet;
using Spectre.Console;
using System.Collections.Generic;

namespace Automatron.GitHub.Commands;

[Command("GitHub", Description = "GitHub action commands")]
public class GitHubCommand
{
    public GitHubCommand(IAnsiConsole console, IEnumerable<Workflow> workflows, IWorkflowEngine workflowEngine)
    {

    }
}
#endif