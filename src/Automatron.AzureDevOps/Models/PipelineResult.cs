using System;

namespace Automatron.AzureDevOps.Models;

#if NET6_0
public record PipelineResult(TimeSpan Elapsed)
{
}
#endif