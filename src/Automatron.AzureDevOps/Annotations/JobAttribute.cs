using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class JobAttribute : Attribute
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Condition { get; set; }

        public string? Emoji { get; set; }

        public Type[]? DependsOn { get; set; }
    }

}
