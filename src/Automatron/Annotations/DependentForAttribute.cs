using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DependentForAttribute : Attribute
    {
        public Type? Controller { get; }

        public string[] Tasks { get; }

        public DependentForAttribute(params string[] tasks)
        {
            Tasks = tasks;
        }

        public DependentForAttribute(Type controller, params string[] tasks)
        {
            Controller = controller;
            Tasks = tasks;
        }
    }
}