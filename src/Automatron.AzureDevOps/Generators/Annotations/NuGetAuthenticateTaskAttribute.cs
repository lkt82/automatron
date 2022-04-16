using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class NuGetAuthenticateTaskAttribute : StepAttribute
    {
        public override Step Create(ISymbol symbol)
        {
            return new NuGetAuthenticateTask
            {
                Name = Name,
                DisplayName = DisplayName
            };
        }
    }
}
