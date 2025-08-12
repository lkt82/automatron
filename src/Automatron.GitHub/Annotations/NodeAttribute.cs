using System;

namespace Automatron.GitHub.Annotations;

public abstract class NodeAttribute : Attribute
{
    public string? Name { get; set; }

    public string? DisplayName { get; set; }

    public string? If { get; set; }

    public string? Emoji { get; set; }
}