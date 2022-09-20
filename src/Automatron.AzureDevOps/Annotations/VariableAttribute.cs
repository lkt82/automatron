using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property, AllowMultiple = true)]
    public class VariableAttribute : Attribute
    {
        public string? Name { get; set; }

        public object? Value { get; set; }

        public string? Description { get; set; }

        [UsedImplicitly]
        public VariableAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }

        [UsedImplicitly]
        public VariableAttribute(string name)
        {
            Name = name;
        }

        [UsedImplicitly]
        public VariableAttribute()
        {
        }
    }
}