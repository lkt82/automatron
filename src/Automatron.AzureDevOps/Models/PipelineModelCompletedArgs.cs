#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelCompletedArgs<T> : EventArgs
{
    public PipelineModelCompletedArgs(T model, TimeSpan elapsed, bool dryRun)
    {
        Model = model;
        Elapsed = elapsed;
        DryRun = dryRun;
    }

    public T Model { get; }

    public TimeSpan Elapsed { get; }

    public bool DryRun { get; }
}
#endif