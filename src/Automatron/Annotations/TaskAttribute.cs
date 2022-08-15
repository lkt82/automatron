using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TaskAttribute : Attribute
    {
        public string? Name { get; }

        public bool Default { get; set; }

        public string? Action { get; set; }

        public TaskAttribute(string name)
        {
            Name = name;
        }

        public TaskAttribute()
        {

        }
    }
}
