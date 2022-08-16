using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,AllowMultiple = true)]
    public class PipelineAttribute : Attribute
    {
        public string? Name { get; }

        public string? YmlName { get; set; }

        public string YmlPath { get; set; } = "./";

        public string? RootPath { get; set; }

        public PipelineAttribute(string name)
        {
            Name = name;
        }

        public PipelineAttribute()
        {
        }
    }
}
