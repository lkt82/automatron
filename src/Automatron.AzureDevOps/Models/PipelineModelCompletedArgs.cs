#if NET8_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelCompletedArgs<T>(T model, TimeSpan elapsed, bool dryRun) : EventArgs
{
    public T Model { get; } = model;

    public TimeSpan Elapsed { get; } = elapsed;

    public bool DryRun { get; } = dryRun;
}
#endif