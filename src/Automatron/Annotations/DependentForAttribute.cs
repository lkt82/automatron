using System;

namespace Automatron.Annotations
{
    public class DependentForAttribute : Attribute
    {
        public DependentForAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public string[] Targets { get; }
    }
}