#if NET8_0
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Middleware;

namespace Automatron.AzureDevOps;

public class AzureDevOpsRunner<T> : AutomationRunner<T> where T : AzureDevOpsCommand
{
    public AzureDevOpsRunner()
    {
        this.UseAzureDevOps();
    }
}

public class AzureDevOpsRunner : AzureDevOpsRunner<AzureDevOpsCommand>
{
}

#endif