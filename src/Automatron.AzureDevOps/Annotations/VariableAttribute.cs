using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
    public class VariableAttribute : Attribute
    {
        public string? Name { get; set; }

        public object? Value { get; set; }

        [UsedImplicitly]
        public VariableAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }

        [UsedImplicitly]
        public VariableAttribute()
        {
        }
    }
}