#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelCompletedArgs<T> : EventArgs
{
    public PipelineModelCompletedArgs(T model, TimeSpan elapsed)
    {
        Model = model;
        Elapsed = elapsed;
    }

    public T Model { get; }

    public TimeSpan Elapsed { get; }
}
#endif