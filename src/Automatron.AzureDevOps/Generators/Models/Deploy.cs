using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models;

public sealed class Deploy
{
    public IEnumerable<Step>? Steps { get; set; }
}