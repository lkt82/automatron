using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,AllowMultiple = true)]
    public class PipelineAttribute : Attribute
    {
        public string? Name { get; }

        public string? DisplayName { get; set; }

        public string? YmlName { get; set; }

        public string YmlDir { get; set; } = "./";

        public string? RootDir { get; set; }

        public PipelineAttribute(string name)
        {
            Name = name;
        }

        public PipelineAttribute()
        {
        }
    }
}
