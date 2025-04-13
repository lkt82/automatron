#if NET8_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelFailedArgs<T>(T model, TimeSpan elapsed, Exception exception, bool dryRun)
    : EventArgs
{
    public Exception Exception { get; } = exception;

    public T Model { get; } = model;

    public TimeSpan Elapsed { get; } = elapsed;

    public bool DryRun { get; } = dryRun;
}
#endif