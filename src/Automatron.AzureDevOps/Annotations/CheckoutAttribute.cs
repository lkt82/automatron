using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckoutAttribute : NodeAttribute
    {
        public string Source { get; }

        public CheckoutAttribute(string source)
        {
            Source = source;
        }

        //public override Step Create(ISymbol symbol, IJob job)
        //{
        //    return new CheckoutTask(job,Source);
        //}
    }
}