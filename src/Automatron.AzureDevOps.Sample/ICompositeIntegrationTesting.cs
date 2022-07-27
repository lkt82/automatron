﻿using Automatron.Annotations;
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
        public const string Environment = "InfrastructureIntegration";

        [DeploymentJob(nameof(Integration), Environment, TimeoutInMinutes = 180)]
        [Download(DownloadSource.None)]
        [Checkout(CheckoutSource.Self)]
        [NuGetAuthenticate]
        [Task]
        public void SetupIntegration()
        {
        }


        [DeploymentJob(nameof(Integration), Environment, TimeoutInMinutes = 180,DependsOn = new []{nameof(SetupIntegration)})]
        [Download(DownloadSource.None)]
        [Checkout(CheckoutSource.Self)]
        [NuGetAuthenticate]
        [Task]
        public void TeardownIntegration()
        {
        }

        [Stage(It)]
        [DependentOn(nameof(SetupIntegration),nameof(TeardownIntegration))]
        public void Integration()
        {
        }
    }
}