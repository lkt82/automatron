using System;

namespace Automatron.AzureDevOps.Annotations;

[AttributeUsage(AttributeTargets.Method)]
public class CheckoutAttribute : NodeAttribute
{
    public string Source { get; }

    public int FetchDepth { get; set; } = 1;

    public bool Clean { get; set; }

    public bool Lfs { get; set; } 

    public bool Submodules { get; set; }

    public string? Path { get; set; }

    public bool PersistCredentials { get; set; }

    public CheckoutAttribute(string source)
    {
        Source = source;
    }
}