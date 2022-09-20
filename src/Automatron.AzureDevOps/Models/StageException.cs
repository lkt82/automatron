#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatron.AzureDevOps.Models;

public class StageException : AggregateException
{
    public IEnumerable<JobException> JobExceptions => InnerExceptions.Cast<JobException>();

    public Stage Stage { get; }

    public TimeSpan Elapsed { get; }

    public StageException(Stage stage, IEnumerable<JobException> jobExceptions,TimeSpan elapsed) :base(jobExceptions)
    {
        Stage = stage;
        Elapsed = elapsed;
    }
}
#endif