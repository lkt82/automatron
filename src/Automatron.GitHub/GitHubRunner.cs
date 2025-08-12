#if NET8_0
using Automatron.GitHub.Commands;
using Automatron.GitHub.Middleware;

namespace Automatron.GitHub;

public class GitHubRunner<T> : AutomationRunner<T> where T : GitHubCommand
{
    public GitHubRunner()
    {
        this.UseGitHub();
    }
}

public class GitHubRunner : GitHubRunner<GitHubCommand>;

#endif