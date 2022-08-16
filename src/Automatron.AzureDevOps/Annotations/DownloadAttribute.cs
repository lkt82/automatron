using System;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;

namespace Automatron.AzureDevOps.Generators.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DownloadAttribute: NodeAttribute
    {
        public string Source { get; }

        public DownloadAttribute(string source)
        {
            Source = source;
        }

        //public override Step Create(ISymbol symbol, IJob job)
        //{
        //    return new DownloadTask(job,Source)
        //    {
        //        Name = Name,
        //        DisplayName = DisplayName,
        //        Source = Source
        //    };
        //}
    }
}
