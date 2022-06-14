using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DependentForAttribute : Attribute
    {
        public DependentForAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public string[] Targets { get; }
    }
}