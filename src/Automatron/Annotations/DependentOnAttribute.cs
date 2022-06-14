using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class DependentOnAttribute : Attribute
    {
        public Type? Controller { get; }

        public string[] Targets { get; }

        public DependentOnAttribute(params string[] targets)
        {
            Targets = targets;
        }

        public DependentOnAttribute(Type controller,params string[] targets)
        {
            Controller = controller;
            Targets = targets;
        }
    }
}