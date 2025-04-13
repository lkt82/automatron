using Automatron;
using Automatron.AzureDevOps.Commands;
using Automatron.AzureDevOps.Middleware;
using Automatron.Commands;
using CommandDotNet;

return await new AutomationRunner<Cli>().UseAzureDevOps().ConfigureServices(c =>
{
    //var pipeline = new Pipeline("Ci", p => new[]
    //{
    //    new Stage("Testing",p,s=> new []
    //    {
    //        new Job("Deployment1",s,j=>
    //        {
    //            var steps = new[]
    //            {
    //                new Step("Init", j,
    //                    nameof(PulumiDeploymentStage.DeploymentJob.Init)),
    //                new Step("Preview", j,
    //                    nameof(PulumiDeploymentStage.DeploymentJob.Preview)),
    //                new Step("Update", j,
    //                    nameof(PulumiDeploymentStage.DeploymentJob.Update))
    //            };
    //            return steps;
    //        },typeof(PulumiDeploymentStage.DeploymentJob)),
    //        new Job("Deployment2",s,j=>
    //        {
    //            var steps = new[]
    //            {
    //                new Step(j,nameof(PulumiDeploymentStage.DeploymentJob.Init)),
    //                new Step(j,nameof(PulumiDeploymentStage.DeploymentJob.Preview)),
    //                new Step(j,nameof(PulumiDeploymentStage.DeploymentJob.Update))
    //            };

    //            return steps;
    //        },typeof(PulumiDeploymentStage.DeploymentJob))
    //    },typeof(DeployToTesting))
    //}, typeof(ContinuousDeployment));

    //c.AddSingleton<IEnumerable<Pipeline>>(new[] { pipeline });
}).RunAsync(args);

public class Cli : CompositeCommand
{
    [Subcommand]
    public AzureDevOpsCommand? AzureDevOps { get; set; }
}
