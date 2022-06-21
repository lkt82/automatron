using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DependentForAttribute : Attribute
    {
        public Type? Controller { get; }

        public string[] Targets { get; }

        public DependentForAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public DependentForAttribute(Type controller, params string[] targets)
        {
            Controller = controller;
            Targets = targets;
        }
    }
}