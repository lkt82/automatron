using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class VariableGroupAttribute : Attribute
    {
        public string? Pipeline { get; }

        public string Name { get; }

        [UsedImplicitly]
        public VariableGroupAttribute(string name)
        {
            Name = name;
        }

        public VariableGroupAttribute(string pipeline,string name)
        {
            Pipeline = pipeline;
            Name = name;
        }
    }
}
