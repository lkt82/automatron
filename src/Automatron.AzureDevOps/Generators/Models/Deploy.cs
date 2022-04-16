using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Deploy
    {
        public IList<Step> Steps { get; set; } = new List<Step>();
    }
}