using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models
{
    public interface IDeploymentStrategy
    {
        List<Step> Steps { get; }
    }
}