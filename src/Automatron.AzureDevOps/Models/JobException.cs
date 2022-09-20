#if NET6_0
using System;

namespace Automatron.AzureDevOps.Models;

public class JobException : Exception
{
    public StepException StepException => (InnerException as StepException)!;

    public Job Job { get; }

    public TimeSpan Elapsed { get; }

    public JobException(Job job, StepException stepException,TimeSpan elapsed) : base("Job failed", stepException)
    {
        Job = job;
        Elapsed = elapsed;
    }
}
#endif