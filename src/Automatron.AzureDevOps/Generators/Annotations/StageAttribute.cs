using System;

namespace Automatron.AzureDevOps.Generators.Annotations
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class StageAttribute : NodeAttribute
    {
        public StageAttribute()
        {
        }

        public StageAttribute(string name)
        {
            Name = name;
        }

        public Type[]? DependsOn { get; set; }
    }
}

