using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class NuGetAuthenticateTaskAttribute : StepAttribute
    {
        public override Step Create(ISymbol symbol, IJob job)
        {
            return new NuGetAuthenticateTask(job)
            {
                Name = Name,
                DisplayName = DisplayName
            };
        }
    }
}
