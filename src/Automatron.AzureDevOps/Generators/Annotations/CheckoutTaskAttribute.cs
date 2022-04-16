using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class CheckoutTaskAttribute : StepAttribute
    {
        public string Source { get; }

        public CheckoutTaskAttribute(string source)
        {
            Source = source;
        }

        public override Step Create(ISymbol symbol)
        {
            return new CheckoutTask(Source);
        }
    }
}