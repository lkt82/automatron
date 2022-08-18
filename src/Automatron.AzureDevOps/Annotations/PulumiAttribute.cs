using System;

namespace Automatron.AzureDevOps.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PulumiAttribute : NodeAttribute
    {
        public string? Command { get; set; }

        public string? Stack { get; set; }

        public string? Cwd { get; set; }

        public string? Args { get; set; }

        //public override Step Create(ISymbol symbol, IJob job)
        //{
        //    if (Command != null || Stack != null || Cwd != null || Args != null)
        //    {
        //        return new PulumiTask(job,new PulumiTask.PulumiTaskInputs
        //        {
        //            Command = Command,
        //            Stack = Stack,
        //            Cwd = Cwd,
        //            Args = Args
        //        })
        //        {
        //            Name = Name,
        //            DisplayName = DisplayName,
        //            Condition = Condition
        //        };
        //    }

        //    return new PulumiTask(job)
        //    {
        //        Name = Name,
        //        DisplayName = DisplayName,
        //        Condition = Condition
        //    };
        //}
    }
}