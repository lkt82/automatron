using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NuGetAuthenticateAttribute : NodeAttribute
    {
        //public override Step Create(ISymbol symbol, IJob job)
        //{
        //    return new NuGetAuthenticateTask(job)
        //    {
        //        Name = Name,
        //        DisplayName = DisplayName
        //    };
        //}
    }
}
