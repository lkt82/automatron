using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;

namespace Automatron.AzureDevOps.Sample;

[Pipeline("CollapsedCi")]
public class CollapsedContinuousDeployment
{
    [Stage]
    [DeploymentJob(Environment = "Testing")]
    public class DeployToTesting(LoggingCommands loggingCommands)
    {
        [Step(Emoji = "🔢")]
        public void Version()
        {
        }
    }

    [Stage]
    [DeploymentJob(Environment = "Production")]
    public class DeployToProduction(LoggingCommands loggingCommands)
    {
        [Step(Emoji = "🔢")]
        public void Version()
        {
        }
    }
}