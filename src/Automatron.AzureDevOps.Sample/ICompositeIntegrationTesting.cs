using Automatron.Annotations;
using Automatron.AzureDevOps.Generators.Annotations;

namespace Automatron.AzureDevOps.Sample
{
    [Pipeline(It)]
    [CiTrigger(It, Disabled = true)]
    [ScheduledTrigger(It,
        "2 0 * * *",
        DisplayName = "Midnight",
        IncludeBranches = new[] { "master" }
    )]
    public interface ICompositeIntegrationTesting
    {
        public const string It = "IntegrationTesting";

        [DeploymentJob(nameof(Integration), "test", TimeoutInMinutes = 180)]
        [DownloadTask(DownloadSource.None)]
        [CheckoutTask(CheckoutSource.Self)]
        [NuGetAuthenticateTask]
        [AutomatronTask]
        public void SetupIntegration()
        {
        }


        [DeploymentJob(nameof(Integration), "test",nameof(SetupIntegration), TimeoutInMinutes = 180)]
        [DownloadTask(DownloadSource.None)]
        [CheckoutTask(CheckoutSource.Self)]
        [NuGetAuthenticateTask]
        [AutomatronTask]
        public void TeardownIntegration()
        {
        }

        [Stage(It)]
        [DependsOn(nameof(SetupIntegration),nameof(TeardownIntegration))]
        public void Integration()
        {
        }
    }
}