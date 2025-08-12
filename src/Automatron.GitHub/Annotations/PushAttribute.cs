using System;

namespace Automatron.GitHub.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class PushAttribute : Attribute
{
    public string[]? Branches { get; set; }

    public string[]? BranchesIgnore { get; set; }

    public string[]? Tags { get; set; }

    public string[]? TagsIgnore { get; set; }

    public string[]? Paths { get; set; }

    public string[]? PathsIgnore { get; set; }
}