#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelStartingArgs<T> : EventArgs
{
    public PipelineModelStartingArgs(T model)
    {
        Model = model;
    }

    public T Model { get; }
}
#endif