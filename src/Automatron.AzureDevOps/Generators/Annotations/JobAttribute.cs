using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{

    [AttributeUsage(AttributeTargets.Method)]
    public class JobTemplateAttribute : Attribute
    {
        public string TemplateType { get; }

        public string? Stage { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public JobTemplateAttribute(Type templateType)
        {
            TemplateType = templateType.FullName ?? throw new InvalidOperationException();
        }

        public JobTemplateAttribute(string stage, Type templateType) :this(templateType)
        {
            Stage = stage;
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class JobAttribute: Attribute
    {
        public string? Stage { get; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string[]? DependsOn { get; set; }

        public string? Condition { get; set; }

        public JobAttribute()
        {
        }

        public JobAttribute(string stage)
        {
            Stage = stage;
        }
    }
}
