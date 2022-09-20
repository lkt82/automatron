#if NET6_0
using System;
using System.Collections.Generic;
using System.Linq;

namespace Automatron.AzureDevOps.Models;

public class PipelineException : AggregateException
{
    public IEnumerable<StageException> StageExceptions => InnerExceptions.Cast<StageException>();

    public Pipeline Pipeline { get; }

    public TimeSpan Elapsed { get; }

    public PipelineException(Pipeline pipeline,IEnumerable<StageException> stageExceptions, TimeSpan elapsed) : base(stageExceptions)
    {
        Pipeline = pipeline;
        Elapsed = elapsed;
    }
}
#endif