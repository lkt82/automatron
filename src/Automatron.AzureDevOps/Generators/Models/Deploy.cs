using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Deploy
{
    public List<Step> Steps { get; set; } = new();
}