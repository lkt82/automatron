using System;

namespace Automatron.AzureDevOps.Annotations
{
    public abstract class NodeAttribute : Attribute
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Condition { get; set; }

        public string? Emoji { get; set; }
    }
}