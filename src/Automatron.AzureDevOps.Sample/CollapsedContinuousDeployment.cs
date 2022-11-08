using Automatron.AzureDevOps.Annotations;
using Automatron.AzureDevOps.Tasks;

namespace Automatron.AzureDevOps.Sample;

[Pipeline("CollapsedCi")]
public class CollapsedContinuousDeployment
{
    [Stage]
    [DeploymentJob(Environment = "Testing")]
    public class DeployToTesting
    {
        private readonly LoggingCommands _loggingCommands;

        public DeployToTesting(LoggingCommands loggingCommands)
        {
            _loggingCommands = loggingCommands;
        }

        [Step(Emoji = "🔢")]
        public void Version()
        {
        }
    }

    [Stage]
    [DeploymentJob(Environment = "Production")]
    public class DeployToProduction
    {
        private readonly LoggingCommands _loggingCommands;

        public DeployToProduction(LoggingCommands loggingCommands)
        {
            _loggingCommands = loggingCommands;
        }

        [Step(Emoji = "🔢")]
        public void Version()
        {
        }
    }
}