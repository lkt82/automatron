#if NET8_0
using System;

namespace Automatron.AzureDevOps.Models;

public class StepException(Step step, TimeSpan elapsed, Exception innerException)
    : Exception("Step failed", innerException)
{
    public Step Step { get; } = step;

    public TimeSpan Elapsed { get; } = elapsed;
}
#endif