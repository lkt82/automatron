using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models
{
    public interface IJob
    {
        string Name { get; }

        IList<Step> Steps { get; }
    }
}