using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class JobAttribute: Attribute
    {
        public string? Stage { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; }

        public string? Condition { get; set; }

        public JobAttribute(params string[] dependencies)
        {
            DependsOn = dependencies;
        }

        public JobAttribute(string stage, params string[] dependencies) : this(dependencies)
        {
            Stage = stage;
        }
    }
}
