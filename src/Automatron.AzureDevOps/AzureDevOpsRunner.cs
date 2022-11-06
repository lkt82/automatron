#if NET6_0
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Middleware;

namespace Automatron.AzureDevOps;

public class AzureDevOpsRunner : AutomationRunner<AzureDevOpsCommand>
{
    public AzureDevOpsRunner()
    {
        this.UseAzureDevOps();
    }
}
#endif