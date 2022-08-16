using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class VariableGroupAttribute : Attribute
    {
        public string Name { get; }

        [UsedImplicitly]
        public VariableGroupAttribute(string name)
        {
            Name = name;
        }
    }
}
