using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class VariableAttribute : Attribute
    {
        public string? Pipeline { get; }

        public string Name { get; }

        public object Value { get; }

        [UsedImplicitly]
        public VariableAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public VariableAttribute(string pipeline, string name,object value)
        {
            Pipeline = pipeline;
            Name = name;
            Value = value;
        }
    }
}