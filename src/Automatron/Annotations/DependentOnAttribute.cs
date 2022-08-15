using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class DependentOnAttribute : Attribute
    {
        public Type? Type { get; }

        public string[] Actions { get; }

        public DependentOnAttribute(params string[] actions)
        {
            Actions = actions;
        }

        public DependentOnAttribute(Type type,params string[] actions)
        {
            Type = type;
            Actions = actions;
        }
    }
}