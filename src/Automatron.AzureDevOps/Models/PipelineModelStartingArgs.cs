#if NET8_0
using System;

namespace Automatron.AzureDevOps.Models;

public class PipelineModelStartingArgs<T>(T model) : EventArgs
{
    public T Model { get; } = model;
}
#endif