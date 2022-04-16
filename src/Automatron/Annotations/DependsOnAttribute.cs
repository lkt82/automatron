using System;

namespace Automatron.Annotations
{
    public class DependsOnAttribute : Attribute
    {
        public DependsOnAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public string[] Targets { get; }
    }
}