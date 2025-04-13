#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Models;

public class StageException(Stage stage, IEnumerable<JobException> jobExceptions, TimeSpan elapsed)
    : AggregateException(jobExceptions)
{
    [UsedImplicitly] 
    public IEnumerable<JobException> JobExceptions => InnerExceptions.Cast<JobException>();

    public Stage Stage { get; } = stage;

    public TimeSpan Elapsed { get; } = elapsed;
}
#endif