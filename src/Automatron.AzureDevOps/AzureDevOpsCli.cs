#if NET6_0
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Middleware;

namespace Automatron.AzureDevOps;

public static class AzureDevOpsCli
{
    public static AutomationRunner New()
    {
        return new AutomationRunner<AzureDevOpsCommand>().UseAzureDevOps();
    }
}
#endif