using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class StageAttribute: Attribute
    {
        public string? Pipeline { get; }

        public string? Name { get; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; }

        public StageAttribute(params string[] dependencies)
        {
            DependsOn = dependencies;
        }

        public StageAttribute(string pipeline, params string[] dependencies) : this(dependencies)
        {
            Pipeline = pipeline;
        }

        public StageAttribute(string name,string pipeline, params string[] dependencies) : this(pipeline,dependencies)
        {
            Name = name;
        }
    }
}
