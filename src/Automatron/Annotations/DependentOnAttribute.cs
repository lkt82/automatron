using System;

namespace Automatron.Annotations
{
    public class DependentOnAttribute : Attribute
    {
        public DependentOnAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public string[] Targets { get; }
    }
}