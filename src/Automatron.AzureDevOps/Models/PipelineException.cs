#if NET8_0
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Automatron.AzureDevOps.Models;

public class PipelineException(Pipeline pipeline, IEnumerable<StageException> stageExceptions, TimeSpan elapsed)
    : AggregateException(stageExceptions)
{
    [UsedImplicitly] 
    public IEnumerable<StageException> StageExceptions => InnerExceptions.Cast<StageException>();

    public Pipeline Pipeline { get; } = pipeline;

    public TimeSpan Elapsed { get; } = elapsed;
}
#endif