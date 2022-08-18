using System.Collections.Generic;

namespace Automatron.AzureDevOps.Models;

public sealed class Deploy
{
    public IEnumerable<Step>? Steps { get; set; }
}