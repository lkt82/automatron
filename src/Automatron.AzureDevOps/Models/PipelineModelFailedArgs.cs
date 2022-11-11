#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelFailedArgs<T> : EventArgs
{
    public PipelineModelFailedArgs(T model, TimeSpan elapsed, Exception exception, bool dryRun)
    {
        Model = model;
        Elapsed = elapsed;
        Exception = exception;
        DryRun = dryRun;
    }

    public Exception Exception { get; }

    public T Model { get; }

    public TimeSpan Elapsed { get; }

    public bool DryRun { get; }
}
#endif