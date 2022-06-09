using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class StageTemplateAttribute : Attribute
    {
        public string TemplateType { get; }

        public string? Pipeline { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public StageTemplateAttribute(Type templateType)
        {
            TemplateType = templateType.FullName ?? throw new InvalidOperationException();
        }

        public StageTemplateAttribute(string pipeline,Type templateType): this(templateType)
        {
            Pipeline = pipeline;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class StageAttribute: Attribute
    {
        public string? Pipeline { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public StageAttribute()
        {
        }

        public StageAttribute(string pipeline)
        {
            Pipeline = pipeline;
        }
    }
}
