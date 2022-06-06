#if NET6_0
using Microsoft.Extensions.DependencyInjection;

namespace Automatron.AzureDevOps;

public static class AzureDevOpsTaskRunnerExtensions
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services)
    {
        return services.AddSingleton<AzureDevOpsTasks>();
    }

    public static TaskRunner<TController> UseAzureDevOps<TController>(this TaskRunner<TController> taskRunner) where TController : class => taskRunner.ConfigureServices(c => c.AddAzureDevOps());
}
#endif