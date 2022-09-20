#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class StepException : Exception
{
    public Step Step { get; }

    public TimeSpan Elapsed { get; }

    public StepException(Step step,TimeSpan elapsed,Exception innerException) :base("Step failed",innerException)
    {
        Step = step;
        Elapsed = elapsed;
    }
}
#endif