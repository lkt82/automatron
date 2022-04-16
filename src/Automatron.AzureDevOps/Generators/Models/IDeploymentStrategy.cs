using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models
{
    public interface IDeploymentStrategy
    {
        IList<Step> Steps { get; }
    }
}