using System.Collections.Generic;

namespace Automatron.AzureDevOps.Generators.Models
{
    public class Deploy
    {
        public List<Step> Steps { get; set; } = new List<Step>();
    }
}