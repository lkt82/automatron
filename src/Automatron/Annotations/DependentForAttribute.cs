using System;

namespace Automatron.Annotations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class DependentForAttribute : Attribute
    {
        public Type? Type { get; }

        public string[] Actions { get; }

        public DependentForAttribute(params string[] actions)
        {
            Actions = actions;
        }

        public DependentForAttribute(Type type, params string[] actions)
        {
            Type = type;
            Actions = actions;
        }
    }
}