using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models;

public interface IDeploymentStrategy
{
    IEnumerable<Step>? Steps { get; set; }
}