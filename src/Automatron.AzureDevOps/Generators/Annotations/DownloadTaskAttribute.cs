using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    public class DownloadTaskAttribute: StepAttribute
    {
        public string Source { get; }

        public DownloadTaskAttribute(string source)
        {
            Source = source;
        }

        public override Step Create(ISymbol symbol)
        {
            return new DownloadTask(Source)
            {
                Name = Name,
                DisplayName = DisplayName,
                Source = Source
            };
        }
    }
}
