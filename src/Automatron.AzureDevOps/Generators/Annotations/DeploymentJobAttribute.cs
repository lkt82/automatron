namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class DeploymentJobAttribute : JobAttribute
    {
        public string Environment { get; }

        public int TimeoutInMinutes { get; set; }

        public DeploymentJobAttribute(string environment, params string[] dependencies) :base(dependencies)
        {
            Environment = environment;
        }

        public DeploymentJobAttribute(string stage, string environment, params string[] dependencies) :base(stage, dependencies)
        {
            Environment = environment;
        }
    }
}