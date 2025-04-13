using System;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
[method: UsedImplicitly]
public class VariableGroupAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}