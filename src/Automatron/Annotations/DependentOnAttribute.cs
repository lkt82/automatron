using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class DependentOnAttribute : Attribute
    {
        public Type? Controller { get; }

        public string[] Tasks { get; }

        public DependentOnAttribute(params string[] tasks)
        {
            Tasks = tasks;
        }

        public DependentOnAttribute(Type controller,params string[] tasks)
        {
            Controller = controller;
            Tasks = tasks;
        }
    }
}