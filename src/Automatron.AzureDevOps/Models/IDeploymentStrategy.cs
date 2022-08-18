using System.Collections.Generic;

namespace Automatron.AzureDevOps.Models;

public interface IDeploymentStrategy
{
    IEnumerable<Step>? Steps { get; set; }
}