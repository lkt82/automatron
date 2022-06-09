using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DeploymentJobAttribute : JobAttribute
    {
        public string Environment { get; }

        public int TimeoutInMinutes { get; set; }

        public DeploymentJobAttribute(string environment)
        {
            Environment = environment;
        }

        public DeploymentJobAttribute(string stage, string environment) :base(stage)
        {
            Environment = environment;
        }
    }
}