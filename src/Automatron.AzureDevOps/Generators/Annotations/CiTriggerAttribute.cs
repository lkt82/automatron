using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class CiTriggerAttribute : Attribute
    {
        [UsedImplicitly]
        public CiTriggerAttribute()
        {

        }

        public CiTriggerAttribute(string pipeline)
        {
            Pipeline = pipeline;
        }

        public string? Pipeline { get;}

        public bool Batch { get; set; }

        public bool Disabled { get; set; }

        public string[]? IncludeBranches { get; set; }

        public string[]? ExcludeBranches { get; set; }

        public string[]? IncludePaths { get; set; }

        public string[]? ExcludePaths { get; set; }
    }
}
